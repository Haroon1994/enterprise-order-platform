using EnterpriseOrderPlatform.Modules.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseOrderPlatform.Modules.Identity.Infrastructure;

public static class IdentityRoleSeeder
{
    /// <summary>Creates any missing platform roles. Safe to run on every startup.</summary>
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

        foreach (var name in Roles.All)
        {
            if (await roleManager.RoleExistsAsync(name))
            {
                continue;
            }

            var result = await roleManager.CreateAsync(new ApplicationRole { Id = Guid.CreateVersion7(), Name = name });
            if (!result.Succeeded)
            {
                var reasons = string.Join(", ", result.Errors.Select(e => e.Code));
                throw new InvalidOperationException($"Could not create role '{name}': {reasons}");
            }
        }
    }
}
