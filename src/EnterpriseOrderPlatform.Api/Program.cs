var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/ping", () => TypedResults.Ok(new PingResponse("enterprise-order-platform", "ok")));

app.Run();

internal sealed record PingResponse(string Service, string Status);

// Lets the integration tests start the app through WebApplicationFactory<Program>.
public partial class Program;
