using System.Security.Claims;
using EnterpriseOrderPlatform.Modules.Identity.Application;
using EnterpriseOrderPlatform.Modules.Identity.Contracts;

namespace EnterpriseOrderPlatform.Host;

internal static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/identity");

        group.MapPost("/register", async (RegisterRequest request, RegisterUser useCase, CancellationToken ct) =>
        {
            var result = await useCase.ExecuteAsync(request, ct);

            if (result.IsSuccess)
            {
                return Results.Json(result.Value, statusCode: StatusCodes.Status201Created);
            }

            var status = result.Error.Code == "email.taken"
                ? StatusCodes.Status409Conflict
                : StatusCodes.Status400BadRequest;

            return Problem("Registration failed", result.Error.Code, result.Error.Description, status);
        });

        group.MapPost("/login", async (LoginRequest request, LoginUser useCase, CancellationToken ct) =>
        {
            var result = await useCase.ExecuteAsync(request, ct);

            if (result.IsSuccess)
            {
                return Results.Ok(result.Value);
            }

            var status = result.Error.Code == "login.locked_out"
                ? StatusCodes.Status429TooManyRequests
                : StatusCodes.Status401Unauthorized;

            return Problem("Login failed", result.Error.Code, result.Error.Description, status);
        });

        group.MapGet("/me", (ClaimsPrincipal user) => Results.Ok(new
        {
            UserId = user.FindFirstValue("sub"),
            Email = user.FindFirstValue("email"),
            Roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value),
        })).RequireAuthorization();

        return app;
    }

    private static IResult Problem(string title, string code, string detail, int status) =>
        Results.Problem(
            title: title,
            detail: detail,
            statusCode: status,
            extensions: new Dictionary<string, object?> { ["code"] = code });
}
