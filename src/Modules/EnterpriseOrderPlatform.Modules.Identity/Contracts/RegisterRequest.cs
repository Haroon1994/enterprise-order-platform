namespace EnterpriseOrderPlatform.Modules.Identity.Contracts;

/// <summary>Public request to register a new customer. It has no role field on purpose.</summary>
public sealed record RegisterRequest(string? Email, string? Password);
