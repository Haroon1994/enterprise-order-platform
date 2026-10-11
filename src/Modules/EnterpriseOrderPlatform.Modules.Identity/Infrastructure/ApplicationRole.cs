using Microsoft.AspNetCore.Identity;

namespace EnterpriseOrderPlatform.Modules.Identity.Infrastructure;

/// <summary>A platform role (Customer, Warehouse, Support, Admin).</summary>
public class ApplicationRole : IdentityRole<Guid>
{
}
