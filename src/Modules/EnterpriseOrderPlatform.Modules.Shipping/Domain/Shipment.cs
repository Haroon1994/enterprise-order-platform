using EnterpriseOrderPlatform.BuildingBlocks;

namespace EnterpriseOrderPlatform.Modules.Shipping.Domain;

public sealed class Shipment
{
    private Shipment() { }

    public Guid Id { get; private set; }

    public Guid OrderId { get; private set; }

    public string Carrier { get; private set; } = string.Empty;

    public string TrackingNumber { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public static Result<Shipment> Create(Guid orderId, string? carrier)
    {
        if (string.IsNullOrWhiteSpace(carrier) || carrier.Trim().Length > 50)
        {
            return Result.Failure<Shipment>(new ResultError("shipment.carrier_invalid", "Carrier is required (max 50 chars)."));
        }

        return Result.Success(new Shipment
        {
            Id = Guid.CreateVersion7(),
            OrderId = orderId,
            Carrier = carrier.Trim(),
            TrackingNumber = "TRK" + Guid.NewGuid().ToString("N")[..12].ToUpperInvariant(),
            CreatedAt = DateTimeOffset.UtcNow,
        });
    }
}
