using EnterpriseOrderPlatform.Modules.Identity.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseOrderPlatform.Modules.Identity.Infrastructure;

public static class IdentityModuleServiceCollectionExtensions
{
    /// <summary>Registers the Identity module: DbContext, Identity stores, use cases.</summary>
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<IdentityModuleDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", IdentityModuleDbContext.Schema)));

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;

                // Length matters more than composition rules.
                options.Password.RequiredLength = 12;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<IdentityModuleDbContext>();

        services.AddScoped<IUserRegistrar, UserRegistrar>();
        services.AddScoped<RegisterUser>();

        return services;
    }
}
