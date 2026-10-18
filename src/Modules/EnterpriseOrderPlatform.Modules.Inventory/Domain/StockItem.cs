using EnterpriseOrderPlatform.BuildingBlocks;

namespace EnterpriseOrderPlatform.Modules.Inventory.Domain;

/// <summary>Stock of one SKU. Available = OnHand - Reserved; neither can go negative.</summary>
public sealed class StockItem
{
    private StockItem() { }

    public Guid Id { get; private set; }

    public string Sku { get; private set; } = string.Empty;

    public int OnHand { get; private set; }

    public int Reserved { get; private set; }

    /// <summary>Maps to PostgreSQL xmin; EF rejects a save if another writer changed the row.</summary>
    public uint Version { get; private set; }

    public int Available => OnHand - Reserved;

    public static Result<StockItem> Create(string? sku, int onHand)
    {
        if (string.IsNullOrWhiteSpace(sku) || sku.Trim().Length > 40)
        {
            return Result.Failure<StockItem>(new ResultError("stock.sku_invalid", "SKU is required (max 40 chars)."));
        }

        if (onHand < 0)
        {
            return Result.Failure<StockItem>(new ResultError("stock.negative", "Stock cannot be negative."));
        }

        return Result.Success(new StockItem
        {
            Id = Guid.CreateVersion7(),
            Sku = sku.Trim().ToUpperInvariant(),
            OnHand = onHand,
        });
    }

    public Result SetOnHand(int onHand)
    {
        if (onHand < Reserved)
        {
            return Result.Failure(new ResultError("stock.below_reserved", "On-hand stock cannot be below reserved stock."));
        }

        OnHand = onHand;
        return Result.Success();
    }

    public Result Reserve(int quantity)
    {
        if (quantity <= 0)
        {
            return Result.Failure(new ResultError("stock.quantity_invalid", "Quantity must be greater than 0."));
        }

        if (quantity > Available)
        {
            return Result.Failure(new ResultError("stock.insufficient", "Not enough stock available."));
        }

        Reserved += quantity;
        return Result.Success();
    }

    public Result Release(int quantity)
    {
        if (quantity <= 0 || quantity > Reserved)
        {
            return Result.Failure(new ResultError("stock.release_invalid", "Cannot release that quantity."));
        }

        Reserved -= quantity;
        return Result.Success();
    }

    /// <summary>Turns a reservation into a real sale (after payment): stock leaves the warehouse.</summary>
    public Result Commit(int quantity)
    {
        if (quantity <= 0 || quantity > Reserved)
        {
            return Result.Failure(new ResultError("stock.commit_invalid", "Cannot commit that quantity."));
        }

        Reserved -= quantity;
        OnHand -= quantity;
        return Result.Success();
    }
}
