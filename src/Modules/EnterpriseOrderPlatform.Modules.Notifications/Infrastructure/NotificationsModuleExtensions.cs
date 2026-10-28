using EnterpriseOrderPlatform.Modules.Notifications.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseOrderPlatform.Modules.Notifications.Infrastructure;

public static class NotificationsModuleExtensions
{
    public static IServiceCollection AddNotificationsModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<NotificationsDbContext>(o =>
            o.UseNpgsql(connectionString, n => n.MigrationsHistoryTable("__EFMigrationsHistory", NotificationsDbContext.Schema)));
        return services;
    }
}
