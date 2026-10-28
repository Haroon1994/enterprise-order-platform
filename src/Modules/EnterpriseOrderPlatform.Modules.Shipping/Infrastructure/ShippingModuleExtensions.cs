using EnterpriseOrderPlatform.Modules.Shipping.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseOrderPlatform.Modules.Shipping.Infrastructure;

public static class ShippingModuleExtensions
{
    public static IServiceCollection AddShippingModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ShippingDbContext>(o =>
            o.UseNpgsql(connectionString, n => n.MigrationsHistoryTable("__EFMigrationsHistory", ShippingDbContext.Schema)));
        return services;
    }
}
