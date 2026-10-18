using EnterpriseOrderPlatform.BuildingBlocks;

namespace EnterpriseOrderPlatform.Modules.Ordering.Application;

public sealed record ProductInfo(string Sku, string Name, decimal Price, string Currency);

/// <summary>Reads current product data. Implemented by the Host using the Catalog module.</summary>
public interface IProductCatalog
{
    Task<ProductInfo?> FindAsync(string sku, CancellationToken cancellationToken);
}

/// <summary>Reserves, releases and commits stock. Implemented by the Host using the Inventory module.</summary>
public interface IStockService
{
    Task<Result> ReserveAsync(string sku, int quantity, CancellationToken cancellationToken);

    Task<Result> ReleaseAsync(string sku, int quantity, CancellationToken cancellationToken);

    Task<Result> CommitAsync(string sku, int quantity, CancellationToken cancellationToken);
}

/// <summary>Charges a customer. The fake implementation never handles real card data.</summary>
public interface IPaymentProvider
{
    Task<Result<string>> ChargeAsync(Guid orderId, decimal amount, string currency, string idempotencyKey, CancellationToken cancellationToken);
}
