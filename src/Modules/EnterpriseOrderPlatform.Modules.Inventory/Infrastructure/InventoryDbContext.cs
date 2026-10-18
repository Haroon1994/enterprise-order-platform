using EnterpriseOrderPlatform.Modules.Inventory.Domain;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseOrderPlatform.Modules.Inventory.Infrastructure;

public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public const string Schema = "inventory";

    public DbSet<StockItem> StockItems => Set<StockItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.Entity<StockItem>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Sku).HasMaxLength(40).IsRequired();
            e.HasIndex(s => s.Sku).IsUnique();
            e.Property(s => s.Version).IsRowVersion();
            e.Ignore(s => s.Available);
            e.ToTable(t => t.HasCheckConstraint("ck_stock_valid", "\"OnHand\" >= 0 AND \"Reserved\" >= 0 AND \"Reserved\" <= \"OnHand\""));
        });
    }
}
