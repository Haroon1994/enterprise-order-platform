using EnterpriseOrderPlatform.Modules.Identity.Application;
using EnterpriseOrderPlatform.Modules.Identity.Contracts;

namespace EnterpriseOrderPlatform.Host;

internal static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/register", async (RegisterRequest request, RegisterUser useCase, CancellationToken ct) =>
        {
            var result = await useCase.ExecuteAsync(request, ct);

            if (result.IsSuccess)
            {
                return Results.Json(result.Value, statusCode: StatusCodes.Status201Created);
            }

            var status = result.Error.Code == "email.taken"
                ? StatusCodes.Status409Conflict
                : StatusCodes.Status400BadRequest;

            return Results.Problem(
                title: "Registration failed",
                detail: result.Error.Description,
                statusCode: status,
                extensions: new Dictionary<string, object?> { ["code"] = result.Error.Code });
        });

        return app;
    }
}
