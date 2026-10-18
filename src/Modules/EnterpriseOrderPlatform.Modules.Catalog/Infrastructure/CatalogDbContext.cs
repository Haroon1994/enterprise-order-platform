using EnterpriseOrderPlatform.Modules.Catalog.Domain;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseOrderPlatform.Modules.Catalog.Infrastructure;

public class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    public const string Schema = "catalog";

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema(Schema);
        builder.Entity<Product>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Sku).HasMaxLength(40).IsRequired();
            e.HasIndex(p => p.Sku).IsUnique();
            e.Property(p => p.Name).HasMaxLength(200).IsRequired();
            e.Property(p => p.Price).HasPrecision(12, 2);
            e.Property(p => p.Currency).HasMaxLength(3);
        });
    }
}
