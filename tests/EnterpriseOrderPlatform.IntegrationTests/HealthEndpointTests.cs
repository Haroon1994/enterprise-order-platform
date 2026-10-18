using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace EnterpriseOrderPlatform.IntegrationTests;

public sealed class HealthEndpointTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public HealthEndpointTests(ApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_Health_ReturnsOkAndHealthy()
    {
        var ct = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/health", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync(ct));
    }
}
