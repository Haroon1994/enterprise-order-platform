using EnterpriseOrderPlatform.BuildingBlocks;
using EnterpriseOrderPlatform.Modules.Identity.Contracts;
using EnterpriseOrderPlatform.Modules.Identity.Domain;

namespace EnterpriseOrderPlatform.Modules.Identity.Application;

/// <summary>Use case: register a new customer.</summary>
public sealed class RegisterUser
{
    public const int MinPasswordLength = 12;
    public const int MaxPasswordLength = 128;

    private readonly IUserRegistrar _registrar;

    public RegisterUser(IUserRegistrar registrar) => _registrar = registrar;

    public async Task<Result<RegisterResponse>> ExecuteAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = EmailAddress.Create(request.Email);
        if (email.IsFailure)
        {
            return Result.Failure<RegisterResponse>(email.Error);
        }

        var password = request.Password;
        if (string.IsNullOrEmpty(password) || password.Length < MinPasswordLength || password.Length > MaxPasswordLength)
        {
            return Result.Failure<RegisterResponse>(new ResultError(
                "password.invalid",
                $"Password must be between {MinPasswordLength} and {MaxPasswordLength} characters."));
        }

        var created = await _registrar.RegisterCustomerAsync(email.Value.Value, password, cancellationToken);

        return created.IsSuccess
            ? Result.Success(new RegisterResponse(created.Value, email.Value.Value))
            : Result.Failure<RegisterResponse>(created.Error);
    }
}
