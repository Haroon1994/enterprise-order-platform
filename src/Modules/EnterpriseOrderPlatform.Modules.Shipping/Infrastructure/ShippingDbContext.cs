using EnterpriseOrderPlatform.Modules.Shipping.Domain;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseOrderPlatform.Modules.Shipping.Infrastructure;

public class ShippingDbContext(DbContextOptions<ShippingDbContext> options) : DbContext(options)
{
    public const string Schema = "shipping";

    public DbSet<Shipment> Shipments => Set<Shipment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.Entity<Shipment>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Id).ValueGeneratedNever();
            e.HasIndex(s => s.OrderId).IsUnique();
            e.Property(s => s.Carrier).HasMaxLength(50).IsRequired();
            e.Property(s => s.TrackingNumber).HasMaxLength(30).IsRequired();
        });
    }
}
