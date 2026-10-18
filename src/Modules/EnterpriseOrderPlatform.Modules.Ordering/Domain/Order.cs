using EnterpriseOrderPlatform.BuildingBlocks;

namespace EnterpriseOrderPlatform.Modules.Ordering.Domain;

public enum OrderStatus
{
    Pending,
    PaymentPending,
    PaymentFailed,
    Paid,
    Processing,
    Shipped,
    Delivered,
    Cancelled,
}

public sealed record OrderLineDraft(string Sku, string Name, decimal UnitPrice, int Quantity);

public sealed class OrderLine
{
    private OrderLine() { }

    internal OrderLine(OrderLineDraft d)
    {
        Id = Guid.CreateVersion7();
        Sku = d.Sku;
        Name = d.Name;
        UnitPrice = d.UnitPrice;
        Quantity = d.Quantity;
    }

    public Guid Id { get; private set; }

    public string Sku { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public decimal UnitPrice { get; private set; }

    public int Quantity { get; private set; }
}

public sealed class Order
{
    private static readonly Dictionary<OrderStatus, OrderStatus[]> Allowed = new()
    {
        [OrderStatus.Pending] = [OrderStatus.PaymentPending, OrderStatus.Cancelled],
        [OrderStatus.PaymentPending] = [OrderStatus.Paid, OrderStatus.PaymentFailed],
        [OrderStatus.PaymentFailed] = [OrderStatus.PaymentPending, OrderStatus.Cancelled],
        [OrderStatus.Paid] = [OrderStatus.Processing, OrderStatus.Cancelled],
        [OrderStatus.Processing] = [OrderStatus.Shipped],
        [OrderStatus.Shipped] = [OrderStatus.Delivered],
        [OrderStatus.Delivered] = [],
        [OrderStatus.Cancelled] = [],
    };

    private readonly List<OrderLine> _lines = [];

    private Order() { }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string IdempotencyKey { get; private set; } = string.Empty;

    public OrderStatus Status { get; private set; }

    public decimal Total { get; private set; }

    public string Currency { get; private set; } = "EUR";

    public string? PaymentReference { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public IReadOnlyCollection<OrderLine> Lines => _lines;

    public static Result<Order> Create(Guid userId, string idempotencyKey, IReadOnlyList<OrderLineDraft> lines)
    {
        if (lines.Count == 0)
        {
            return Result.Failure<Order>(new ResultError("order.empty", "An order needs at least one line."));
        }

        var order = new Order
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            IdempotencyKey = idempotencyKey,
            Status = OrderStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        foreach (var line in lines)
        {
            order._lines.Add(new OrderLine(line));
        }

        order.Total = decimal.Round(lines.Sum(l => l.UnitPrice * l.Quantity), 2);
        return Result.Success(order);
    }

    public Result TransitionTo(OrderStatus next)
    {
        if (!Allowed[Status].Contains(next))
        {
            return Result.Failure(new ResultError("order.illegal_transition", $"Cannot move from {Status} to {next}."));
        }

        Status = next;
        return Result.Success();
    }

    public Result MarkPaid(string paymentReference)
    {
        var result = TransitionTo(OrderStatus.Paid);
        if (result.IsSuccess)
        {
            PaymentReference = paymentReference;
        }

        return result;
    }
}
