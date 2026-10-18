using EnterpriseOrderPlatform.Modules.Ordering.Domain;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseOrderPlatform.Modules.Ordering.Infrastructure;

public class OrderingDbContext(DbContextOptions<OrderingDbContext> options) : DbContext(options)
{
    public const string Schema = "ordering";

    public DbSet<Cart> Carts => Set<Cart>();

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);

        modelBuilder.Entity<Cart>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Id).ValueGeneratedNever();
            e.HasIndex(c => c.UserId).IsUnique();
            e.HasMany(c => c.Items).WithOne().HasForeignKey("CartId").OnDelete(DeleteBehavior.Cascade);
            e.Navigation(c => c.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<CartItem>(e =>
        {
            e.HasKey(i => i.Id);
            e.Property(i => i.Id).ValueGeneratedNever();
            e.Property(i => i.Sku).HasMaxLength(40).IsRequired();
        });

        modelBuilder.Entity<Order>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.Id).ValueGeneratedNever();
            e.Property(o => o.IdempotencyKey).HasMaxLength(64).IsRequired();
            e.Property(o => o.Status).HasConversion<string>().HasMaxLength(30);
            e.Property(o => o.Total).HasPrecision(12, 2);
            e.Property(o => o.Currency).HasMaxLength(3);
            e.Property(o => o.PaymentReference).HasMaxLength(100);
            e.HasIndex(o => new { o.UserId, o.IdempotencyKey }).IsUnique();
            e.HasMany(o => o.Lines).WithOne().HasForeignKey("OrderId").OnDelete(DeleteBehavior.Cascade);
            e.Navigation(o => o.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<OrderLine>(e =>
        {
            e.HasKey(l => l.Id);
            e.Property(l => l.Id).ValueGeneratedNever();
            e.Property(l => l.Sku).HasMaxLength(40).IsRequired();
            e.Property(l => l.Name).HasMaxLength(200).IsRequired();
            e.Property(l => l.UnitPrice).HasPrecision(12, 2);
        });
    }
}
