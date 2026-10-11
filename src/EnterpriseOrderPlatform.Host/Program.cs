var builder = WebApplication.CreateBuilder(args);

var postgresConnectionString = builder.Configuration.GetConnectionString("Postgres");

var healthChecks = builder.Services.AddHealthChecks();
if (!string.IsNullOrWhiteSpace(postgresConnectionString))
{
    healthChecks.AddNpgSql(postgresConnectionString, name: "postgres", tags: ["ready"]);
}

var app = builder.Build();

app.MapGet("/ping", () => TypedResults.Ok(new PingResponse("enterprise-order-platform", "ok")));
app.MapHealthChecks("/health");

app.Run();

internal sealed record PingResponse(string Service, string Status);

// Lets the integration tests start the app through WebApplicationFactory<Program>.
public partial class Program;
