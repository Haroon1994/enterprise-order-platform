using EnterpriseOrderPlatform.Modules.Shipping.Domain;
using Xunit;

namespace EnterpriseOrderPlatform.UnitTests;

public class ShipmentTests
{
    [Fact]
    public void Create_Valid_GeneratesTrackingNumber()
    {
        var r = Shipment.Create(Guid.NewGuid(), " DHL ");
        Assert.True(r.IsSuccess);
        Assert.Equal("DHL", r.Value.Carrier);
        Assert.StartsWith("TRK", r.Value.TrackingNumber);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_InvalidCarrier_Fails(string? carrier)
    {
        Assert.True(Shipment.Create(Guid.NewGuid(), carrier).IsFailure);
    }
}
