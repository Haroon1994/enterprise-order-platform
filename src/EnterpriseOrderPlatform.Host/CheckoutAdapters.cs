using EnterpriseOrderPlatform.BuildingBlocks;
using EnterpriseOrderPlatform.Modules.Catalog.Infrastructure;
using EnterpriseOrderPlatform.Modules.Inventory.Infrastructure;
using EnterpriseOrderPlatform.Modules.Ordering.Application;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseOrderPlatform.Host;

internal static class CheckoutAdapters
{
    public static IServiceCollection AddCheckoutAdapters(this IServiceCollection services)
    {
        services.AddScoped<IProductCatalog, CatalogAdapter>();
        services.AddScoped<IStockService, StockAdapter>();
        return services;
    }
}

internal sealed class CatalogAdapter(CatalogDbContext db) : IProductCatalog
{
    public async Task<ProductInfo?> FindAsync(string sku, CancellationToken cancellationToken)
    {
        var key = sku.Trim().ToUpperInvariant();
        return await db.Products.AsNoTracking()
            .Where(p => p.Sku == key && p.IsActive)
            .Select(p => new ProductInfo(p.Sku, p.Name, p.Price, p.Currency))
            .FirstOrDefaultAsync(cancellationToken);
    }
}

internal sealed class StockAdapter(InventoryDbContext db) : IStockService
{
    public Task<Result> ReserveAsync(string sku, int quantity, CancellationToken cancellationToken) =>
        ApplyAsync(sku, s => s.Reserve(quantity), cancellationToken);

    public Task<Result> ReleaseAsync(string sku, int quantity, CancellationToken cancellationToken) =>
        ApplyAsync(sku, s => s.Release(quantity), cancellationToken);

    public Task<Result> CommitAsync(string sku, int quantity, CancellationToken cancellationToken) =>
        ApplyAsync(sku, s => s.Commit(quantity), cancellationToken);

    private async Task<Result> ApplyAsync(
        string sku, Func<Modules.Inventory.Domain.StockItem, Result> action, CancellationToken cancellationToken)
    {
        var key = sku.Trim().ToUpperInvariant();
        var item = await db.StockItems.FirstOrDefaultAsync(s => s.Sku == key, cancellationToken);
        if (item is null)
        {
            return Result.Failure(new ResultError("stock.unknown", "No stock record for this SKU."));
        }

        var result = action(item);
        if (result.IsFailure)
        {
            return result;
        }

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Result.Failure(new ResultError("stock.conflict", "Stock changed concurrently. Retry."));
        }

        return Result.Success();
    }
}
