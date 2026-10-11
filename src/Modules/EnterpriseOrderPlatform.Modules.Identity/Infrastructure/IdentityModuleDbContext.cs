using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseOrderPlatform.Modules.Identity.Infrastructure;

/// <summary>Database context of the Identity module. All tables live in the "identity" schema.</summary>
public class IdentityModuleDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public const string Schema = "identity";

    public IdentityModuleDbContext(DbContextOptions<IdentityModuleDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema(Schema);
    }
}
