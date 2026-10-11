namespace EnterpriseOrderPlatform.Modules.Identity.Domain;

/// <summary>The role names used for authorization across the platform.</summary>
public static class Roles
{
    public const string Customer = "Customer";
    public const string Warehouse = "Warehouse";
    public const string Support = "Support";
    public const string Admin = "Admin";

    public static readonly IReadOnlyList<string> All = [Customer, Warehouse, Support, Admin];
}
