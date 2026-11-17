using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ecommerce.IntegrationTests.Fixtures;

namespace Ecommerce.IntegrationTests.Api;

public class AuthenticationTests(DatabaseFixture fixture) : ApiTestBase(fixture)
{
    // The real CurrentUserProvider turns the token claims into the customer the services act as
    [Fact]
    public async Task GetPurchases_WithToken_ReturnsOnlyOwnPurchases()
    {
        var otherCustomer = await TestData.CustomerAsync();
        var purchase = await TestData.PurchaseAsync();
        await TestData.PurchaseAsync(otherCustomer);

        var response = await Client().GetAsync("/api/purchases");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(purchase.Id, Assert.Single(body.GetProperty("data").EnumerateArray()).GetProperty("id").GetInt32());
    }

    [Fact]
    public async Task Login_IssuesTokenAcceptedByApi()
    {
        await TestData.CustomerAsync(email: "customer@test.com");
        var login = await Client(null).PostAsJsonAsync("/api/auth/login", new { email = "customer@test.com", password = TestData.DefaultPassword });
        var token = (await login.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("token").GetString()!;

        var response = await ClientWithToken(token).GetAsync("/api/cart");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("GET", "/api/cart")]
    [InlineData("GET", "/api/orders")]
    [InlineData("GET", "/api/purchases")]
    [InlineData("POST", "/api/purchases")]
    [InlineData("POST", "/api/orders/1/cancel")]
    [InlineData("GET", "/api/sellers/me/orders")]
    [InlineData("POST", "/api/products")]
    public async Task ProtectedEndpoints_WithoutToken_AreUnauthorized(string method, string url)
    {
        var response = await Client(null).SendAsync(new HttpRequestMessage(new HttpMethod(method), url));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/products")]
    [InlineData("/api/productcategories")]
    public async Task AnonymousEndpoints_WithoutToken_ReturnOk(string url)
    {
        var response = await Client(null).GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // Control for the rejection tests below: a token built by TestTokens is accepted when nothing is changed
    [Fact]
    public async Task GetCart_WithValidToken_ReturnsOk()
    {
        var response = await ClientWithToken(TestTokens.Create(Customer)).GetAsync("/api/cart");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // Expired beyond the default clock skew of 5 minutes
    [Fact]
    public async Task GetCart_WithExpiredToken_IsUnauthorized()
    {
        var token = TestTokens.Create(Customer, expires: DateTime.UtcNow.AddMinutes(-10));

        var response = await ClientWithToken(token).GetAsync("/api/cart");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetCart_WithTokenSignedByAnotherKey_IsUnauthorized()
    {
        var token = TestTokens.Create(Customer, key: "another-signing-key-not-a-secret-0123456789");

        var response = await ClientWithToken(token).GetAsync("/api/cart");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetCart_WithTokenFromAnotherIssuer_IsUnauthorized()
    {
        var token = TestTokens.Create(Customer, issuer: "Another.Issuer");

        var response = await ClientWithToken(token).GetAsync("/api/cart");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
