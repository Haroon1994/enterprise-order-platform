using EnterpriseOrderPlatform.BuildingBlocks;

namespace EnterpriseOrderPlatform.Modules.Catalog.Domain;

public sealed class Product
{
    private Product() { }

    public Guid Id { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public string Currency { get; private set; } = "EUR";
    public bool IsActive { get; private set; } = true;

    public static Result<Product> Create(string? sku, string? name, decimal price)
    {
        if (string.IsNullOrWhiteSpace(sku) || sku.Trim().Length > 40)
        {
            return Result.Failure<Product>(new ResultError("product.sku_invalid", "SKU is required (max 40 chars)."));
        }

        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 200)
        {
            return Result.Failure<Product>(new ResultError("product.name_invalid", "Name is required (max 200 chars)."));
        }

        if (price <= 0 || price > 1_000_000m)
        {
            return Result.Failure<Product>(new ResultError("product.price_invalid", "Price must be greater than 0."));
        }

        return Result.Success(new Product
        {
            Id = Guid.CreateVersion7(),
            Sku = sku.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Price = decimal.Round(price, 2),
        });
    }

    public void Deactivate() => IsActive = false;
}
