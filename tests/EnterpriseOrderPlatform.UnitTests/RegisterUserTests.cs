using EnterpriseOrderPlatform.BuildingBlocks;
using EnterpriseOrderPlatform.Modules.Identity.Application;
using EnterpriseOrderPlatform.Modules.Identity.Contracts;
using Xunit;

namespace EnterpriseOrderPlatform.UnitTests;

public class RegisterUserTests
{
    private sealed class FakeRegistrar : IUserRegistrar
    {
        public int Calls { get; private set; }

        public Task<Result<Guid>> RegisterCustomerAsync(string email, string password, CancellationToken cancellationToken)
        {
            Calls++;
            return Task.FromResult(Result.Success(Guid.NewGuid()));
        }
    }

    [Fact]
    public async Task ValidRequest_Succeeds_AndNormalizesEmail()
    {
        var registrar = new FakeRegistrar();
        var useCase = new RegisterUser(registrar);

        var result = await useCase.ExecuteAsync(
            new RegisterRequest("  User@Example.COM ", "a-long-enough-password"),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal("user@example.com", result.Value.Email);
        Assert.Equal(1, registrar.Calls);
    }

    [Fact]
    public async Task InvalidEmail_Fails_WithoutCallingRegistrar()
    {
        var registrar = new FakeRegistrar();
        var useCase = new RegisterUser(registrar);

        var result = await useCase.ExecuteAsync(
            new RegisterRequest("not-an-email", "a-long-enough-password"),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(0, registrar.Calls);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("short")]
    public async Task InvalidPassword_Fails_WithoutCallingRegistrar(string? password)
    {
        var registrar = new FakeRegistrar();
        var useCase = new RegisterUser(registrar);

        var result = await useCase.ExecuteAsync(
            new RegisterRequest("user@example.com", password),
            TestContext.Current.CancellationToken);

        Assert.Equal("password.invalid", result.Error.Code);
        Assert.Equal(0, registrar.Calls);
    }
}
