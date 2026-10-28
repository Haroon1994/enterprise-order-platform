using EnterpriseOrderPlatform.Modules.Notifications.Domain;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseOrderPlatform.Modules.Notifications.Infrastructure;

public class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : DbContext(options)
{
    public const string Schema = "notifications";

    public DbSet<NotificationLog> Logs => Set<NotificationLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.Entity<NotificationLog>(e =>
        {
            e.HasKey(n => n.Id);
            e.Property(n => n.Id).ValueGeneratedNever();
            e.Property(n => n.Type).HasMaxLength(50).IsRequired();
            e.Property(n => n.Message).HasMaxLength(500).IsRequired();
            e.HasIndex(n => n.UserId);
        });
    }
}
