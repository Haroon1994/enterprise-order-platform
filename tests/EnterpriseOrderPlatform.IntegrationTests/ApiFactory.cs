using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EnterpriseOrderPlatform.IntegrationTests;

/// <summary>
/// Starts the app in the "Testing" environment, so user-secrets (and therefore the
/// real development database) are not loaded.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }
}
