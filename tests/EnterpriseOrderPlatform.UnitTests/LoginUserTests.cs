using EnterpriseOrderPlatform.BuildingBlocks;
using EnterpriseOrderPlatform.Modules.Identity.Application;
using EnterpriseOrderPlatform.Modules.Identity.Contracts;
using Xunit;

namespace EnterpriseOrderPlatform.UnitTests;

public class LoginUserTests
{
    private sealed class FakeAuthenticator(bool succeed) : IUserAuthenticator
    {
        public int Calls { get; private set; }

        public Task<Result<AuthenticatedUser>> AuthenticateAsync(string email, string password, CancellationToken cancellationToken)
        {
            Calls++;
            return Task.FromResult(succeed
                ? Result.Success(new AuthenticatedUser(Guid.NewGuid(), email, ["Customer"]))
                : Result.Failure<AuthenticatedUser>(new ResultError("login.invalid_credentials", "bad")));
        }
    }

    private sealed class FakeTokens : ITokenService
    {
        public LoginResponse CreateAccessToken(AuthenticatedUser user) =>
            new("token-for-" + user.Email, DateTimeOffset.UtcNow.AddMinutes(15));
    }

    [Fact]
    public async Task ValidCredentials_ReturnToken()
    {
        var useCase = new LoginUser(new FakeAuthenticator(true), new FakeTokens());

        var result = await useCase.ExecuteAsync(new LoginRequest("A@B.com", "whatever-password"), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal("token-for-a@b.com", result.Value.AccessToken);
    }

    [Fact]
    public async Task WrongPassword_Fails()
    {
        var useCase = new LoginUser(new FakeAuthenticator(false), new FakeTokens());

        var result = await useCase.ExecuteAsync(new LoginRequest("a@b.com", "wrong"), TestContext.Current.CancellationToken);

        Assert.Equal("login.invalid_credentials", result.Error.Code);
    }

    [Theory]
    [InlineData(null, "pw")]
    [InlineData("not-an-email", "pw")]
    [InlineData("a@b.com", null)]
    [InlineData("a@b.com", "")]
    public async Task MalformedInput_FailsWithSameErrorAndSkipsAuthenticator(string? email, string? password)
    {
        var authenticator = new FakeAuthenticator(true);
        var useCase = new LoginUser(authenticator, new FakeTokens());

        var result = await useCase.ExecuteAsync(new LoginRequest(email, password), TestContext.Current.CancellationToken);

        Assert.Equal("login.invalid_credentials", result.Error.Code);
        Assert.Equal(0, authenticator.Calls);
    }
}
