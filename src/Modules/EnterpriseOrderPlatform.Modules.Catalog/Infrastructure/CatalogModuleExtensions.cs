using EnterpriseOrderPlatform.Modules.Catalog.Contracts;
using EnterpriseOrderPlatform.Modules.Catalog.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseOrderPlatform.Modules.Catalog.Infrastructure;

public static class CatalogModuleExtensions
{
    public static IServiceCollection AddCatalogModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<CatalogDbContext>(o =>
            o.UseNpgsql(connectionString, n => n.MigrationsHistoryTable("__EFMigrationsHistory", CatalogDbContext.Schema)));
        return services;
    }

    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/catalog");

        group.MapGet("/products", async (CatalogDbContext db, int? page, int? pageSize, CancellationToken ct) =>
        {
            var p = Math.Max(page ?? 1, 1);
            var size = Math.Clamp(pageSize ?? 20, 1, 100);
            var items = await db.Products.AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Skip((p - 1) * size).Take(size)
                .Select(x => new ProductResponse(x.Id, x.Sku, x.Name, x.Price, x.Currency))
                .ToListAsync(ct);
            return Results.Ok(items);
        });

        group.MapGet("/products/{id:guid}", async (Guid id, CatalogDbContext db, CancellationToken ct) =>
        {
            var x = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id && p.IsActive, ct);
            return x is null ? Results.NotFound() : Results.Ok(new ProductResponse(x.Id, x.Sku, x.Name, x.Price, x.Currency));
        });

        group.MapPost("/products", async (CreateProductRequest req, CatalogDbContext db, CancellationToken ct) =>
        {
            var created = Product.Create(req.Sku, req.Name, req.Price);
            if (created.IsFailure)
            {
                return Results.Problem(title: "Invalid product", detail: created.Error.Description,
                    statusCode: 400, extensions: new Dictionary<string, object?> { ["code"] = created.Error.Code });
            }

            var p = created.Value;
            if (await db.Products.AnyAsync(x => x.Sku == p.Sku, ct))
            {
                return Results.Problem(title: "Duplicate SKU", detail: "A product with this SKU exists.", statusCode: 409);
            }

            db.Products.Add(p);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/catalog/products/{p.Id}", new ProductResponse(p.Id, p.Sku, p.Name, p.Price, p.Currency));
        }).RequireAuthorization(policy => policy.RequireRole("Admin"));

        return app;
    }
}
