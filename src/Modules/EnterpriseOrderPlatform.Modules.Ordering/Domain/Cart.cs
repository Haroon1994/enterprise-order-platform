using EnterpriseOrderPlatform.BuildingBlocks;

namespace EnterpriseOrderPlatform.Modules.Ordering.Domain;

public sealed class CartItem
{
    private CartItem() { }

    internal CartItem(string sku, int quantity)
    {
        Id = Guid.CreateVersion7();
        Sku = sku;
        Quantity = quantity;
    }

    public Guid Id { get; private set; }

    public string Sku { get; private set; } = string.Empty;

    public int Quantity { get; internal set; }
}

/// <summary>A customer's shopping cart. It holds SKUs and quantities only, never prices.</summary>
public sealed class Cart
{
    public const int MaxQuantityPerLine = 99;
    public const int MaxLines = 50;

    private readonly List<CartItem> _items = [];

    private Cart() { }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public IReadOnlyCollection<CartItem> Items => _items;

    public static Cart Create(Guid userId) => new() { Id = Guid.CreateVersion7(), UserId = userId };

    public Result SetItem(string? sku, int quantity)
    {
        if (string.IsNullOrWhiteSpace(sku) || sku.Trim().Length > 40)
        {
            return Result.Failure(new ResultError("cart.sku_invalid", "SKU is required (max 40 chars)."));
        }

        if (quantity < 1 || quantity > MaxQuantityPerLine)
        {
            return Result.Failure(new ResultError("cart.quantity_invalid", $"Quantity must be between 1 and {MaxQuantityPerLine}."));
        }

        var key = sku.Trim().ToUpperInvariant();
        var existing = _items.Find(i => i.Sku == key);
        if (existing is not null)
        {
            existing.Quantity = quantity;
            return Result.Success();
        }

        if (_items.Count >= MaxLines)
        {
            return Result.Failure(new ResultError("cart.full", "The cart has too many items."));
        }

        _items.Add(new CartItem(key, quantity));
        return Result.Success();
    }

    public Result RemoveItem(string? sku)
    {
        var key = sku?.Trim().ToUpperInvariant();
        var existing = _items.Find(i => i.Sku == key);
        if (existing is null)
        {
            return Result.Failure(new ResultError("cart.item_not_found", "Item is not in the cart."));
        }

        _items.Remove(existing);
        return Result.Success();
    }

    public void Clear() => _items.Clear();
}
