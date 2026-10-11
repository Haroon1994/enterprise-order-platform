namespace EnterpriseOrderPlatform.BuildingBlocks;

/// <summary>A stable error code plus a human-readable description.</summary>
public sealed record ResultError(string Code, string Description)
{
    public static readonly ResultError None = new(string.Empty, string.Empty);
}
