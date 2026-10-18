using EnterpriseOrderPlatform.Modules.Ordering.Domain;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseOrderPlatform.Modules.Ordering.Infrastructure;

public class OrderingDbContext(DbContextOptions<OrderingDbContext> options) : DbContext(options)
{
    public const string Schema = "ordering";

    public DbSet<Cart> Carts => Set<Cart>();

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
    }
}
