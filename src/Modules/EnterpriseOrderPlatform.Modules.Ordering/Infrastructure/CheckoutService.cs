using EnterpriseOrderPlatform.BuildingBlocks;
using EnterpriseOrderPlatform.Modules.Ordering.Application;
using EnterpriseOrderPlatform.Modules.Ordering.Contracts;
using EnterpriseOrderPlatform.Modules.Ordering.Domain;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseOrderPlatform.Modules.Ordering.Infrastructure;

/// <summary>Checkout: price from catalog, reserve stock, create order, charge, then commit or release.</summary>
public sealed class CheckoutService(
    OrderingDbContext db, IProductCatalog catalog, IStockService stock, IPaymentProvider payments)
{
    public async Task<Result<OrderResponse>> ExecuteAsync(Guid userId, string idempotencyKey, CancellationToken ct)
    {
        var existing = await db.Orders.AsNoTracking().Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.UserId == userId && o.IdempotencyKey == idempotencyKey, ct);
        if (existing is not null)
        {
            return Result.Success(ToResponse(existing));
        }

        var cart = await db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId, ct);
        if (cart is null || cart.Items.Count == 0)
        {
            return Result.Failure<OrderResponse>(new ResultError("checkout.empty_cart", "The cart is empty."));
        }

        var drafts = new List<OrderLineDraft>();
        foreach (var item in cart.Items)
        {
            var product = await catalog.FindAsync(item.Sku, ct);
            if (product is null)
            {
                return Result.Failure<OrderResponse>(new ResultError("checkout.product_unavailable", $"Product {item.Sku} is unavailable."));
            }

            drafts.Add(new OrderLineDraft(product.Sku, product.Name, product.Price, item.Quantity));
        }

        var reserved = new List<OrderLineDraft>();
        foreach (var draft in drafts)
        {
            var result = await stock.ReserveAsync(draft.Sku, draft.Quantity, ct);
            if (result.IsFailure)
            {
                await ReleaseAllAsync(reserved, ct);
                return Result.Failure<OrderResponse>(new ResultError("checkout.out_of_stock", $"Not enough stock for {draft.Sku}."));
            }

            reserved.Add(draft);
        }

        var created = Order.Create(userId, idempotencyKey, drafts);
        if (created.IsFailure)
        {
            await ReleaseAllAsync(reserved, ct);
            return Result.Failure<OrderResponse>(created.Error);
        }

        var order = created.Value;
        order.TransitionTo(OrderStatus.PaymentPending);
        db.Orders.Add(order);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            await ReleaseAllAsync(reserved, ct);
            return Result.Failure<OrderResponse>(new ResultError("checkout.duplicate", "This checkout is already in progress."));
        }

        var payment = await payments.ChargeAsync(order.Id, order.Total, order.Currency, idempotencyKey, ct);
        if (payment.IsSuccess)
        {
            order.MarkPaid(payment.Value);
            foreach (var line in reserved)
            {
                await stock.CommitAsync(line.Sku, line.Quantity, ct);
            }

            cart.Clear();
        }
        else
        {
            order.TransitionTo(OrderStatus.PaymentFailed);
            await ReleaseAllAsync(reserved, ct);
        }

        await db.SaveChangesAsync(ct);
        return Result.Success(ToResponse(order));
    }

    public static OrderResponse ToResponse(Order o) => new(
        o.Id,
        o.Status.ToString(),
        o.Total,
        o.Currency,
        o.Lines.Select(l => new OrderLineResponse(l.Sku, l.Name, l.UnitPrice, l.Quantity)).ToList());

    private async Task ReleaseAllAsync(List<OrderLineDraft> reserved, CancellationToken ct)
    {
        foreach (var line in reserved)
        {
            await stock.ReleaseAsync(line.Sku, line.Quantity, ct);
        }
    }
}
