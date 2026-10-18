using EnterpriseOrderPlatform.Modules.Ordering.Contracts;
using EnterpriseOrderPlatform.Modules.Ordering.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace EnterpriseOrderPlatform.Modules.Ordering.Infrastructure;

public static class OrderingModuleExtensions
{
    public static IServiceCollection AddOrderingModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<OrderingDbContext>(o =>
            o.UseNpgsql(connectionString, n => n.MigrationsHistoryTable("__EFMigrationsHistory", OrderingDbContext.Schema)));
        services.AddScoped<CheckoutService>();
        services.AddSingleton<Application.IPaymentProvider, FakePaymentProvider>();
        return services;
    }

    public static IEndpointRouteBuilder MapOrderingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/cart").RequireAuthorization();

        group.MapGet("/", async (ClaimsPrincipal user, OrderingDbContext db, CancellationToken ct) =>
        {
            var userId = GetUserId(user);
            if (userId is null)
            {
                return Results.Unauthorized();
            }

            var cart = await db.Carts.AsNoTracking().Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId, ct);
            return Results.Ok(ToResponse(cart));
        });

        group.MapPut("/items/{sku}", async (string sku, SetCartItemRequest req, ClaimsPrincipal user, OrderingDbContext db, CancellationToken ct) =>
        {
            var userId = GetUserId(user);
            if (userId is null)
            {
                return Results.Unauthorized();
            }

            var cart = await db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId, ct);
            if (cart is null)
            {
                cart = Cart.Create(userId.Value);
                db.Carts.Add(cart);
            }

            var result = cart.SetItem(sku, req.Quantity);
            if (result.IsFailure)
            {
                return Problem(result.Error.Code, result.Error.Description, 400);
            }

            await db.SaveChangesAsync(ct);
            return Results.Ok(ToResponse(cart));
        });

        group.MapDelete("/items/{sku}", async (string sku, ClaimsPrincipal user, OrderingDbContext db, CancellationToken ct) =>
        {
            var userId = GetUserId(user);
            if (userId is null)
            {
                return Results.Unauthorized();
            }

            var cart = await db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId, ct);
            if (cart is null)
            {
                return Problem("cart.item_not_found", "Item is not in the cart.", 404);
            }

            var result = cart.RemoveItem(sku);
            if (result.IsFailure)
            {
                return Problem(result.Error.Code, result.Error.Description, 404);
            }

            await db.SaveChangesAsync(ct);
            return Results.Ok(ToResponse(cart));
        });

        return app;
    }

    private static Guid? GetUserId(ClaimsPrincipal user) =>
        Guid.TryParse(user.FindFirst("sub")?.Value, out var id) ? id : null;

    private static CartResponse ToResponse(Cart? cart) =>
        new(cart is null ? [] : cart.Items.Select(i => new CartItemResponse(i.Sku, i.Quantity)).ToList());

    private static IResult Problem(string code, string detail, int status) =>
        Results.Problem(title: "Cart update failed", detail: detail, statusCode: status,
            extensions: new Dictionary<string, object?> { ["code"] = code });
}
