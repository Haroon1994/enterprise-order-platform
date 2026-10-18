using EnterpriseOrderPlatform.BuildingBlocks;
using EnterpriseOrderPlatform.Modules.Identity.Application;
using Microsoft.AspNetCore.Identity;

namespace EnterpriseOrderPlatform.Modules.Identity.Infrastructure;

internal sealed class UserAuthenticator : IUserAuthenticator
{
    private static readonly ResultError Invalid =
        new("login.invalid_credentials", "Email or password is incorrect.");

    private static readonly ResultError LockedOut =
        new("login.locked_out", "Too many failed attempts. Try again later.");

    private readonly UserManager<ApplicationUser> _userManager;

    public UserAuthenticator(UserManager<ApplicationUser> userManager) => _userManager = userManager;

    public async Task<Result<AuthenticatedUser>> AuthenticateAsync(
        string email, string password, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return Result.Failure<AuthenticatedUser>(Invalid);
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return Result.Failure<AuthenticatedUser>(LockedOut);
        }

        if (!await _userManager.CheckPasswordAsync(user, password))
        {
            await _userManager.AccessFailedAsync(user);
            return Result.Failure<AuthenticatedUser>(Invalid);
        }

        await _userManager.ResetAccessFailedCountAsync(user);
        var roles = await _userManager.GetRolesAsync(user);

        return Result.Success(new AuthenticatedUser(user.Id, user.Email ?? email, roles.ToList()));
    }
}
