namespace EnterpriseOrderPlatform.Modules.Identity.Contracts;

public sealed record LoginResponse(string AccessToken, DateTimeOffset ExpiresAt);
