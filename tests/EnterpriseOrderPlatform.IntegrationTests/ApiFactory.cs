using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;
using Xunit;

namespace EnterpriseOrderPlatform.IntegrationTests;

/// <summary>
/// Starts the app in the "Testing" environment against a throwaway PostgreSQL container.
/// User-secrets are not loaded, so the development database is never touched.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder("postgres:17").Build();

    public string ConnectionString => _db.GetConnectionString();

    public async ValueTask InitializeAsync() => await _db.StartAsync();

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _db.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Postgres", _db.GetConnectionString());
        builder.UseSetting("Jwt:Key", "integration-test-signing-key-at-least-32-characters-long!");
    }
}
