using EnterpriseOrderPlatform.BuildingBlocks;

namespace EnterpriseOrderPlatform.Modules.Identity.Application;

public sealed record AuthenticatedUser(Guid Id, string Email, IReadOnlyList<string> Roles);

/// <summary>Checks credentials. Implemented by Infrastructure.</summary>
public interface IUserAuthenticator
{
    Task<Result<AuthenticatedUser>> AuthenticateAsync(string email, string password, CancellationToken cancellationToken);
}
