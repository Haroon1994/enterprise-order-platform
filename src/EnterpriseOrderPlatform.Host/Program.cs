using EnterpriseOrderPlatform.Host;
using EnterpriseOrderPlatform.Modules.Identity.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var postgresConnectionString = builder.Configuration.GetConnectionString("Postgres");
var hasDatabase = !string.IsNullOrWhiteSpace(postgresConnectionString);

var healthChecks = builder.Services.AddHealthChecks();
if (hasDatabase)
{
    healthChecks.AddNpgSql(postgresConnectionString!, name: "postgres", tags: ["ready"]);
    builder.Services.AddIdentityModule(postgresConnectionString!);
}

var app = builder.Build();

if (hasDatabase && app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    await IdentityRoleSeeder.SeedAsync(scope.ServiceProvider);
}

app.MapGet("/ping", () => TypedResults.Ok(new PingResponse("enterprise-order-platform", "ok")));
app.MapHealthChecks("/health");

if (hasDatabase)
{
    app.MapIdentityEndpoints();
}

app.Run();

internal sealed record PingResponse(string Service, string Status);

// Lets the integration tests start the app through WebApplicationFactory<Program>.
public partial class Program;
