using EnterpriseOrderPlatform.Modules.Identity.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseOrderPlatform.Modules.Identity.Infrastructure;

public static class IdentityModuleServiceCollectionExtensions
{
    /// <summary>Registers the Identity module: DbContext, Identity stores, use cases, tokens.</summary>
    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services, string connectionString, IConfiguration configuration)
    {
        services.AddDbContext<IdentityModuleDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", IdentityModuleDbContext.Schema)));

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Password.RequiredLength = 12;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;

                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<IdentityModuleDbContext>();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddScoped<IUserRegistrar, UserRegistrar>();
        services.AddScoped<IUserAuthenticator, UserAuthenticator>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddScoped<RegisterUser>();
        services.AddScoped<LoginUser>();

        return services;
    }
}
