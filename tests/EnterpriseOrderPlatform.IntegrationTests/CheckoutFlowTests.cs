using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using EnterpriseOrderPlatform.Modules.Catalog.Infrastructure;
using EnterpriseOrderPlatform.Modules.Identity.Infrastructure;
using EnterpriseOrderPlatform.Modules.Inventory.Infrastructure;
using EnterpriseOrderPlatform.Modules.Notifications.Infrastructure;
using EnterpriseOrderPlatform.Modules.Ordering.Infrastructure;
using EnterpriseOrderPlatform.Modules.Shipping.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EnterpriseOrderPlatform.IntegrationTests;

public sealed class CheckoutFlowTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string Password = "correct-horse-battery";

    [Fact]
    public async Task Checkout_IsIdempotent_AndReducesStock()
    {
        var ct = TestContext.Current.CancellationToken;
        await PrepareAsync(ct);
        using var admin = await LoginAsync("admin1@example.com", ["Admin", "Warehouse"], ct);
        await SeedProductAsync(admin, "FLOW-1", 10m, 10, ct);

        using var buyer = await LoginAsync("buyer1@example.com", [], ct);
        await buyer.PutAsJsonAsync("/api/cart/items/FLOW-1", new { quantity = 2 }, ct);

        var first = await Checkout(buyer, "key-a", ct);
        var retry = await Checkout(buyer, "key-a", ct);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(await first.Content.ReadAsStringAsync(ct), await retry.Content.ReadAsStringAsync(ct));

        var stock = await admin.GetStringAsync("/api/inventory/stock/FLOW-1", ct);
        Assert.Contains("\"onHand\":8", stock, StringComparison.Ordinal);
    }

    [Fact]
    public async Task LastUnit_IsSoldToOnlyOneBuyer()
    {
        var ct = TestContext.Current.CancellationToken;
        await PrepareAsync(ct);
        using var admin = await LoginAsync("admin2@example.com", ["Admin", "Warehouse"], ct);
        await SeedProductAsync(admin, "LAST-1", 10m, 1, ct);

        using var a = await LoginAsync("buyer2a@example.com", [], ct);
        using var b = await LoginAsync("buyer2b@example.com", [], ct);
        await a.PutAsJsonAsync("/api/cart/items/LAST-1", new { quantity = 1 }, ct);
        await b.PutAsJsonAsync("/api/cart/items/LAST-1", new { quantity = 1 }, ct);

        var results = await Task.WhenAll(Checkout(a, "k-a", ct), Checkout(b, "k-b", ct));

        var ok = results.Count(r => r.StatusCode == HttpStatusCode.OK
            && r.Content.ReadAsStringAsync(ct).Result.Contains("\"status\":\"Paid\"", StringComparison.Ordinal));
        Assert.Equal(1, ok);

        var stock = await admin.GetStringAsync("/api/inventory/stock/LAST-1", ct);
        Assert.Contains("\"onHand\":0", stock, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Order_IsNotVisibleToAnotherUser()
    {
        var ct = TestContext.Current.CancellationToken;
        await PrepareAsync(ct);
        using var admin = await LoginAsync("admin3@example.com", ["Admin", "Warehouse"], ct);
        await SeedProductAsync(admin, "OWN-1", 5m, 5, ct);

        using var owner = await LoginAsync("owner3@example.com", [], ct);
        using var other = await LoginAsync("other3@example.com", [], ct);
        await owner.PutAsJsonAsync("/api/cart/items/OWN-1", new { quantity = 1 }, ct);
        var body = await (await Checkout(owner, "k-own", ct)).Content.ReadAsStringAsync(ct);
        var id = System.Text.Json.JsonDocument.Parse(body).RootElement.GetProperty("id").GetGuid();

        Assert.Equal(HttpStatusCode.OK, (await owner.GetAsync($"/api/orders/{id}", ct)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await other.GetAsync($"/api/orders/{id}", ct)).StatusCode);
    }

    private static async Task<HttpResponseMessage> Checkout(HttpClient client, string key, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/orders/checkout");
        request.Headers.Add("Idempotency-Key", key);
        return await client.SendAsync(request, ct);
    }

    private static async Task SeedProductAsync(HttpClient admin, string sku, decimal price, int stock, CancellationToken ct)
    {
        var created = await admin.PostAsJsonAsync("/api/catalog/products", new { sku, name = sku, price }, ct);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var put = await admin.PutAsJsonAsync($"/api/inventory/stock/{sku}", new { onHand = stock }, ct);
        Assert.Equal(HttpStatusCode.OK, put.StatusCode);
    }

    private async Task<HttpClient> LoginAsync(string email, string[] extraRoles, CancellationToken ct)
    {
        var client = factory.CreateClient();
        var register = await client.PostAsJsonAsync("/api/identity/register", new { email, password = Password }, ct);
        Assert.Equal(HttpStatusCode.Created, register.StatusCode);

        if (extraRoles.Length > 0)
        {
            using var scope = factory.Services.CreateScope();
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await users.FindByEmailAsync(email);
            await users.AddToRolesAsync(user!, extraRoles);
        }

        var login = await client.PostAsJsonAsync("/api/identity/login", new { email, password = Password }, ct);
        var token = (await login.Content.ReadFromJsonAsync<Dictionary<string, object>>(ct))!["accessToken"].ToString();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private async Task PrepareAsync(CancellationToken ct)
    {
        using var scope = factory.Services.CreateScope();
        var sp = scope.ServiceProvider;
        foreach (var context in new DbContext[]
        {
            sp.GetRequiredService<IdentityModuleDbContext>(),
            sp.GetRequiredService<CatalogDbContext>(),
            sp.GetRequiredService<InventoryDbContext>(),
            sp.GetRequiredService<OrderingDbContext>(),
            sp.GetRequiredService<ShippingDbContext>(),
            sp.GetRequiredService<NotificationsDbContext>(),
        })
        {
            await context.Database.MigrateAsync(ct);
        }

        await IdentityRoleSeeder.SeedAsync(sp);
    }
}
