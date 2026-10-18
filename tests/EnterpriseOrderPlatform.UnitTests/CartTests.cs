using EnterpriseOrderPlatform.Modules.Ordering.Domain;
using Xunit;

namespace EnterpriseOrderPlatform.UnitTests;

public class CartTests
{
    [Fact]
    public void SetItem_AddsNewLine_WithNormalizedSku()
    {
        var cart = Cart.Create(Guid.NewGuid());
        Assert.True(cart.SetItem(" ab-1 ", 2).IsSuccess);
        Assert.Equal("AB-1", Assert.Single(cart.Items).Sku);
    }

    [Fact]
    public void SetItem_SameSku_ReplacesQuantity()
    {
        var cart = Cart.Create(Guid.NewGuid());
        cart.SetItem("A", 2);
        cart.SetItem("a", 5);
        Assert.Equal(5, Assert.Single(cart.Items).Quantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(100)]
    public void SetItem_InvalidQuantity_Fails(int quantity)
    {
        Assert.True(Cart.Create(Guid.NewGuid()).SetItem("A", quantity).IsFailure);
    }

    [Fact]
    public void RemoveItem_Existing_Succeeds()
    {
        var cart = Cart.Create(Guid.NewGuid());
        cart.SetItem("A", 1);
        Assert.True(cart.RemoveItem("a").IsSuccess);
        Assert.Empty(cart.Items);
    }

    [Fact]
    public void RemoveItem_Missing_Fails()
    {
        Assert.True(Cart.Create(Guid.NewGuid()).RemoveItem("A").IsFailure);
    }
}
