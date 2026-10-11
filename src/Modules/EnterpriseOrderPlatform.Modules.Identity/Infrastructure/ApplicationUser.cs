using Microsoft.AspNetCore.Identity;

namespace EnterpriseOrderPlatform.Modules.Identity.Infrastructure;

/// <summary>A platform user, stored by ASP.NET Core Identity.</summary>
public class ApplicationUser : IdentityUser<Guid>
{
}
