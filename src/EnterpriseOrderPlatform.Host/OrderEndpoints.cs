using System.Security.Claims;
using EnterpriseOrderPlatform.Modules.Ordering.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseOrderPlatform.Host;

internal static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders").RequireAuthorization();

        group.MapPost("/checkout", async (
            [FromHeader(Name = "Idempotency-Key")] string? key,
            ClaimsPrincipal user,
            CheckoutService checkout,
            CancellationToken ct) =>
        {
            if (!Guid.TryParse(user.FindFirst("sub")?.Value, out var userId))
            {
                return Results.Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(key) || key.Length > 64)
            {
                return Problem("checkout.key_required", "Send an Idempotency-Key header (max 64 chars).", 400);
            }

            var result = await checkout.ExecuteAsync(userId, key.Trim(), ct);
            if (result.IsSuccess)
            {
                return Results.Ok(result.Value);
            }

            var status = result.Error.Code is "checkout.out_of_stock" or "checkout.duplicate" ? 409 : 400;
            return Problem(result.Error.Code, result.Error.Description, status);
        });

        group.MapGet("/{id:guid}", async (Guid id, ClaimsPrincipal user, OrderingDbContext db, CancellationToken ct) =>
        {
            if (!Guid.TryParse(user.FindFirst("sub")?.Value, out var userId))
            {
                return Results.Unauthorized();
            }

            // Filtering by UserId means other users' orders look like "not found" (no BOLA leak).
            var order = await db.Orders.AsNoTracking().Include(o => o.Lines)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId, ct);

            return order is null ? Results.NotFound() : Results.Ok(CheckoutService.ToResponse(order));
        });

        return app;
    }

    private static IResult Problem(string code, string detail, int status) =>
        Results.Problem(title: "Checkout failed", detail: detail, statusCode: status,
            extensions: new Dictionary<string, object?> { ["code"] = code });
}
