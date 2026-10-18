using EnterpriseOrderPlatform.Modules.Catalog.Domain;
using Xunit;

namespace EnterpriseOrderPlatform.UnitTests;

public class ProductTests
{
    [Fact]
    public void Create_Valid_NormalizesSku()
    {
        var r = Product.Create(" ab-1 ", "Widget", 9.999m);
        Assert.True(r.IsSuccess);
        Assert.Equal("AB-1", r.Value.Sku);
        Assert.Equal(10.00m, r.Value.Price);
    }

    [Theory]
    [InlineData(null, "n", 1)]
    [InlineData("s", "", 1)]
    [InlineData("s", "n", 0)]
    [InlineData("s", "n", -5)]
    public void Create_Invalid_Fails(string? sku, string? name, double price)
    {
        Assert.True(Product.Create(sku, name, (decimal)price).IsFailure);
    }
}
