using EnterpriseOrderPlatform.Modules.Inventory.Contracts;
using EnterpriseOrderPlatform.Modules.Inventory.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseOrderPlatform.Modules.Inventory.Infrastructure;

public static class InventoryModuleExtensions
{
    public static IServiceCollection AddInventoryModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<InventoryDbContext>(o =>
            o.UseNpgsql(connectionString, n => n.MigrationsHistoryTable("__EFMigrationsHistory", InventoryDbContext.Schema)));
        return services;
    }

    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/inventory").RequireAuthorization(p => p.RequireRole("Warehouse", "Admin"));

        group.MapGet("/stock/{sku}", async (string sku, InventoryDbContext db, CancellationToken ct) =>
        {
            var key = sku.Trim().ToUpperInvariant();
            var s = await db.StockItems.AsNoTracking().FirstOrDefaultAsync(x => x.Sku == key, ct);
            return s is null ? Results.NotFound() : Results.Ok(ToResponse(s));
        });

        group.MapPut("/stock/{sku}", async (string sku, SetStockRequest req, InventoryDbContext db, CancellationToken ct) =>
        {
            var key = sku.Trim().ToUpperInvariant();
            var item = await db.StockItems.FirstOrDefaultAsync(x => x.Sku == key, ct);

            if (item is null)
            {
                var created = StockItem.Create(key, req.OnHand);
                if (created.IsFailure)
                {
                    return Problem(created.Error.Code, created.Error.Description, 400);
                }

                item = created.Value;
                db.StockItems.Add(item);
            }
            else
            {
                var set = item.SetOnHand(req.OnHand);
                if (set.IsFailure)
                {
                    return Problem(set.Error.Code, set.Error.Description, 400);
                }
            }

            try
            {
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException)
            {
                return Problem("stock.conflict", "The stock was changed by someone else. Retry.", 409);
            }

            return Results.Ok(ToResponse(item));
        });

        return app;
    }

    private static StockResponse ToResponse(StockItem s) => new(s.Sku, s.OnHand, s.Reserved, s.Available);

    private static IResult Problem(string code, string detail, int status) =>
        Results.Problem(title: "Stock update failed", detail: detail, statusCode: status,
            extensions: new Dictionary<string, object?> { ["code"] = code });
}
