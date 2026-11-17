using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ecommerce.IntegrationTests.Fixtures;

namespace Ecommerce.IntegrationTests.Api;

public class AuthorizationTests(DatabaseFixture fixture) : ApiTestBase(fixture)
{
    // The RequireSeller policy answers before the action runs, so the ids and bodies don't need to exist
    [Theory]
    [InlineData("POST", "/api/products")]
    [InlineData("PUT", "/api/products/1")]
    [InlineData("PATCH", "/api/products/1/stock")]
    [InlineData("DELETE", "/api/products/1")]
    [InlineData("POST", "/api/products/1/categories/1")]
    [InlineData("DELETE", "/api/products/1/categories/1")]
    [InlineData("POST", "/api/productcategories")]
    [InlineData("PUT", "/api/productcategories/1")]
    [InlineData("DELETE", "/api/productcategories/1")]
    [InlineData("POST", "/api/orders/1/ship")]
    [InlineData("GET", "/api/sellers/me/orders")]
    public async Task SellerEndpoints_AsCustomer_AreForbidden(string method, string url)
    {
        var response = await Client(Customer).SendAsync(new HttpRequestMessage(new HttpMethod(method), url));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_AsSeller_ReturnsCreatedWithLocation()
    {
        var response = await Client(Seller).PostAsJsonAsync("/api/products", new { name = "Test Product", price = 10.0m, stockBalance = 5 });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var id = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();
        Assert.EndsWith($"/api/products/{id}", response.Headers.Location!.ToString(), StringComparison.OrdinalIgnoreCase);
    }
}
