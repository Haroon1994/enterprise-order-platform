using EnterpriseOrderPlatform.Modules.Identity.Domain;
using Xunit;

namespace EnterpriseOrderPlatform.UnitTests;

public class EmailAddressTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("  User@Example.COM  ")]
    public void Create_WithValidEmail_Succeeds(string input)
    {
        var result = EmailAddress.Create(input);

        Assert.True(result.IsSuccess);
        Assert.Equal("user@example.com", result.Value.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("no-at-sign")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("user@nodot")]
    [InlineData("a@@b.com")]
    [InlineData("us er@example.com")]
    public void Create_WithInvalidEmail_Fails(string? input)
    {
        var result = EmailAddress.Create(input);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_WithTooLongEmail_Fails()
    {
        var result = EmailAddress.Create(new string('a', 250) + "@x.com");

        Assert.Equal("email.too_long", result.Error.Code);
    }

    [Fact]
    public void Roles_ContainsTheFourPlatformRoles()
    {
        Assert.Equal(4, Roles.All.Count);
        Assert.Contains(Roles.Customer, Roles.All);
    }
}
