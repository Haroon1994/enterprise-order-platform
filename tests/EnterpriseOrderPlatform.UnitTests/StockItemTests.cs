using EnterpriseOrderPlatform.Modules.Inventory.Domain;
using Xunit;

namespace EnterpriseOrderPlatform.UnitTests;

public class StockItemTests
{
    private static StockItem NewItem(int onHand) => StockItem.Create("sku-1", onHand).Value;

    [Fact]
    public void Create_NormalizesSku_AndStartsUnreserved()
    {
        var item = NewItem(5);
        Assert.Equal("SKU-1", item.Sku);
        Assert.Equal(5, item.Available);
    }

    [Fact]
    public void Reserve_ReducesAvailable()
    {
        var item = NewItem(5);
        Assert.True(item.Reserve(3).IsSuccess);
        Assert.Equal(2, item.Available);
        Assert.Equal(3, item.Reserved);
    }

    [Fact]
    public void Reserve_MoreThanAvailable_Fails_AndChangesNothing()
    {
        var item = NewItem(2);
        var result = item.Reserve(3);
        Assert.Equal("stock.insufficient", result.Error.Code);
        Assert.Equal(0, item.Reserved);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Reserve_InvalidQuantity_Fails(int quantity)
    {
        Assert.True(NewItem(5).Reserve(quantity).IsFailure);
    }

    [Fact]
    public void Release_ReturnsStockToAvailable()
    {
        var item = NewItem(5);
        item.Reserve(4);
        Assert.True(item.Release(1).IsSuccess);
        Assert.Equal(2, item.Available);
    }

    [Fact]
    public void Release_MoreThanReserved_Fails()
    {
        var item = NewItem(5);
        item.Reserve(1);
        Assert.True(item.Release(2).IsFailure);
    }

    [Fact]
    public void Commit_RemovesStockAndReservation()
    {
        var item = NewItem(5);
        item.Reserve(2);
        Assert.True(item.Commit(2).IsSuccess);
        Assert.Equal(3, item.OnHand);
        Assert.Equal(0, item.Reserved);
    }

    [Fact]
    public void SetOnHand_BelowReserved_Fails()
    {
        var item = NewItem(5);
        item.Reserve(4);
        Assert.True(item.SetOnHand(3).IsFailure);
        Assert.Equal(5, item.OnHand);
    }
}
