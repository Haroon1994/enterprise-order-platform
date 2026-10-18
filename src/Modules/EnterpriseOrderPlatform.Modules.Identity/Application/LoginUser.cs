using EnterpriseOrderPlatform.BuildingBlocks;
using EnterpriseOrderPlatform.Modules.Identity.Contracts;
using EnterpriseOrderPlatform.Modules.Identity.Domain;

namespace EnterpriseOrderPlatform.Modules.Identity.Application;

/// <summary>Use case: log in with email and password and receive an access token.</summary>
public sealed class LoginUser
{
    private static readonly ResultError InvalidCredentials =
        new("login.invalid_credentials", "Email or password is incorrect.");

    private readonly IUserAuthenticator _authenticator;
    private readonly ITokenService _tokens;

    public LoginUser(IUserAuthenticator authenticator, ITokenService tokens)
    {
        _authenticator = authenticator;
        _tokens = tokens;
    }

    public async Task<Result<LoginResponse>> ExecuteAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = EmailAddress.Create(request.Email);
        if (email.IsFailure || string.IsNullOrEmpty(request.Password))
        {
            // Same answer as a wrong password, so callers cannot probe which emails exist.
            return Result.Failure<LoginResponse>(InvalidCredentials);
        }

        var user = await _authenticator.AuthenticateAsync(email.Value.Value, request.Password, cancellationToken);

        return user.IsSuccess
            ? Result.Success(_tokens.CreateAccessToken(user.Value))
            : Result.Failure<LoginResponse>(user.Error);
    }
}
