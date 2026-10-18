using System.Text;
using EnterpriseOrderPlatform.Host;
using EnterpriseOrderPlatform.Modules.Catalog.Infrastructure;
using EnterpriseOrderPlatform.Modules.Identity.Infrastructure;
using EnterpriseOrderPlatform.Modules.Inventory.Infrastructure;
using EnterpriseOrderPlatform.Modules.Ordering.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var postgresConnectionString = builder.Configuration.GetConnectionString("Postgres");
var hasDatabase = !string.IsNullOrWhiteSpace(postgresConnectionString);

var healthChecks = builder.Services.AddHealthChecks();
if (hasDatabase)
{
    healthChecks.AddNpgSql(postgresConnectionString!, name: "postgres", tags: ["ready"]);
    builder.Services.AddIdentityModule(postgresConnectionString!, builder.Configuration);
    builder.Services.AddCatalogModule(postgresConnectionString!);
    builder.Services.AddInventoryModule(postgresConnectionString!);
    builder.Services.AddOrderingModule(postgresConnectionString!);

    var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
    if (jwt.Key.Length < 32)
    {
        throw new InvalidOperationException("Jwt:Key must be configured and at least 32 characters long.");
    }

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwt.Issuer,
                ValidateAudience = true,
                ValidAudience = jwt.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30),
                NameClaimType = "sub",
                RoleClaimType = System.Security.Claims.ClaimTypes.Role,
            };
        });
    builder.Services.AddAuthorization();
}

var app = builder.Build();

if (hasDatabase && app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    await IdentityRoleSeeder.SeedAsync(scope.ServiceProvider);
}

if (hasDatabase)
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.MapGet("/ping", () => TypedResults.Ok(new PingResponse("enterprise-order-platform", "ok")));
app.MapHealthChecks("/health");

if (hasDatabase)
{
    app.MapIdentityEndpoints();
    app.MapCatalogEndpoints();
    app.MapInventoryEndpoints();
    app.MapOrderingEndpoints();
}

app.Run();

internal sealed record PingResponse(string Service, string Status);

// Lets the integration tests start the app through WebApplicationFactory<Program>.
public partial class Program;
