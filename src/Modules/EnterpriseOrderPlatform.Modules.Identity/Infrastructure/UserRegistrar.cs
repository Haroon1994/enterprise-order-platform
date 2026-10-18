using EnterpriseOrderPlatform.BuildingBlocks;
using EnterpriseOrderPlatform.Modules.Identity.Application;
using EnterpriseOrderPlatform.Modules.Identity.Domain;
using Microsoft.AspNetCore.Identity;

namespace EnterpriseOrderPlatform.Modules.Identity.Infrastructure;

/// <summary>Creates users through ASP.NET Core Identity. The role is always Customer.</summary>
internal sealed class UserRegistrar : IUserRegistrar
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserRegistrar(UserManager<ApplicationUser> userManager) => _userManager = userManager;

    public async Task<Result<Guid>> RegisterCustomerAsync(string email, string password, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = new ApplicationUser { Id = Guid.CreateVersion7(), UserName = email, Email = email };

        var created = await _userManager.CreateAsync(user, password);
        if (!created.Succeeded)
        {
            var taken = created.Errors.Any(e => e.Code is "DuplicateUserName" or "DuplicateEmail");
            return taken
                ? Result.Failure<Guid>(new ResultError("email.taken", "This email cannot be used."))
                : Result.Failure<Guid>(new ResultError("password.rejected", "The password does not meet the requirements."));
        }

        var roleAdded = await _userManager.AddToRoleAsync(user, Roles.Customer);
        if (!roleAdded.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return Result.Failure<Guid>(new ResultError("registration.failed", "Registration could not be completed."));
        }

        return Result.Success(user.Id);
    }
}
