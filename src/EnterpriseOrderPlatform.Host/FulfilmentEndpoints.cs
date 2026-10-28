using EnterpriseOrderPlatform.Modules.Notifications.Domain;
using EnterpriseOrderPlatform.Modules.Notifications.Infrastructure;
using EnterpriseOrderPlatform.Modules.Ordering.Domain;
using EnterpriseOrderPlatform.Modules.Ordering.Infrastructure;
using EnterpriseOrderPlatform.Modules.Shipping.Domain;
using EnterpriseOrderPlatform.Modules.Shipping.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseOrderPlatform.Host;

public sealed record ShipRequest(string? Carrier);

internal static class FulfilmentEndpoints
{
    public static IEndpointRouteBuilder MapFulfilmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/fulfilment").RequireAuthorization(p => p.RequireRole("Warehouse", "Admin"));

        group.MapPost("/orders/{id:guid}/process", async (Guid id, OrderingDbContext orders, NotificationsDbContext notes, CancellationToken ct) =>
        {
            var order = await orders.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);
            if (order is null)
            {
                return Results.NotFound();
            }

            var result = order.TransitionTo(OrderStatus.Processing);
            if (result.IsFailure)
            {
                return Problem(result.Error.Code, result.Error.Description, 409);
            }

            await orders.SaveChangesAsync(ct);
            return Results.Ok(new { order.Id, Status = order.Status.ToString() });
        });

        group.MapPost("/orders/{id:guid}/ship", async (
            Guid id, ShipRequest req, OrderingDbContext orders, ShippingDbContext shipping, NotificationsDbContext notes, CancellationToken ct) =>
        {
            var order = await orders.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);
            if (order is null)
            {
                return Results.NotFound();
            }

            var shipment = Shipment.Create(order.Id, req.Carrier);
            if (shipment.IsFailure)
            {
                return Problem(shipment.Error.Code, shipment.Error.Description, 400);
            }

            var moved = order.TransitionTo(OrderStatus.Shipped);
            if (moved.IsFailure)
            {
                return Problem(moved.Error.Code, moved.Error.Description, 409);
            }

            shipping.Shipments.Add(shipment.Value);
            notes.Logs.Add(NotificationLog.Create(order.UserId, "order.shipped",
                $"Your order {order.Id} has shipped with {shipment.Value.Carrier}. Tracking: {shipment.Value.TrackingNumber}"));

            await shipping.SaveChangesAsync(ct);
            await notes.SaveChangesAsync(ct);
            await orders.SaveChangesAsync(ct);
            return Results.Ok(new { order.Id, Status = order.Status.ToString(), shipment.Value.TrackingNumber });
        });

        group.MapPost("/orders/{id:guid}/deliver", async (Guid id, OrderingDbContext orders, NotificationsDbContext notes, CancellationToken ct) =>
        {
            var order = await orders.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);
            if (order is null)
            {
                return Results.NotFound();
            }

            var result = order.TransitionTo(OrderStatus.Delivered);
            if (result.IsFailure)
            {
                return Problem(result.Error.Code, result.Error.Description, 409);
            }

            notes.Logs.Add(NotificationLog.Create(order.UserId, "order.delivered", $"Your order {order.Id} was delivered."));
            await notes.SaveChangesAsync(ct);
            await orders.SaveChangesAsync(ct);
            return Results.Ok(new { order.Id, Status = order.Status.ToString() });
        });

        return app;
    }

    private static IResult Problem(string code, string detail, int status) =>
        Results.Problem(title: "Fulfilment failed", detail: detail, statusCode: status,
            extensions: new Dictionary<string, object?> { ["code"] = code });
}
