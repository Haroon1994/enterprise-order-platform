namespace EnterpriseOrderPlatform.Modules.Identity.Infrastructure;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Key { get; set; } = string.Empty;

    public string Issuer { get; set; } = "enterprise-order-platform";

    public string Audience { get; set; } = "enterprise-order-platform";

    public int ExpiryMinutes { get; set; } = 15;
}
