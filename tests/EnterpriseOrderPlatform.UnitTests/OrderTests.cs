using EnterpriseOrderPlatform.Modules.Ordering.Domain;
using Xunit;

namespace EnterpriseOrderPlatform.UnitTests;

public class OrderTests
{
    private static Order NewOrder() =>
        Order.Create(Guid.NewGuid(), "key-1", [new OrderLineDraft("A", "Widget", 10m, 2), new OrderLineDraft("B", "Gadget", 5.5m, 1)]).Value;

    [Fact]
    public void Create_ComputesTotal_AndStartsPending()
    {
        var order = NewOrder();
        Assert.Equal(25.5m, order.Total);
        Assert.Equal(OrderStatus.Pending, order.Status);
    }

    [Fact]
    public void Create_WithoutLines_Fails()
    {
        Assert.True(Order.Create(Guid.NewGuid(), "k", []).IsFailure);
    }

    [Fact]
    public void HappyPath_ReachesDelivered()
    {
        var order = NewOrder();
        Assert.True(order.TransitionTo(OrderStatus.PaymentPending).IsSuccess);
        Assert.True(order.MarkPaid("ref").IsSuccess);
        Assert.True(order.TransitionTo(OrderStatus.Processing).IsSuccess);
        Assert.True(order.TransitionTo(OrderStatus.Shipped).IsSuccess);
        Assert.True(order.TransitionTo(OrderStatus.Delivered).IsSuccess);
    }

    [Fact]
    public void PaymentFailed_CanRetryPayment()
    {
        var order = NewOrder();
        order.TransitionTo(OrderStatus.PaymentPending);
        order.TransitionTo(OrderStatus.PaymentFailed);
        Assert.True(order.TransitionTo(OrderStatus.PaymentPending).IsSuccess);
    }

    [Fact]
    public void Pending_CannotJumpToShipped()
    {
        var order = NewOrder();
        var result = order.TransitionTo(OrderStatus.Shipped);
        Assert.Equal("order.illegal_transition", result.Error.Code);
        Assert.Equal(OrderStatus.Pending, order.Status);
    }

    [Fact]
    public void Cancelled_IsTerminal()
    {
        var order = NewOrder();
        order.TransitionTo(OrderStatus.Cancelled);
        Assert.True(order.TransitionTo(OrderStatus.PaymentPending).IsFailure);
    }
}
