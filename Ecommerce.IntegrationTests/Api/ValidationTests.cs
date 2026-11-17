using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ecommerce.Domain.Entities;
using Ecommerce.IntegrationTests.Fixtures;

namespace Ecommerce.IntegrationTests.Api;

public class ValidationTests(DatabaseFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public async Task CreatePurchase_WithoutPaymentMethod_IsInvalid()
    {
        var response = await Client().PostAsJsonAsync("/api/purchases", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.GetProperty("errors").TryGetProperty("PaymentMethod", out _));
    }

    [Theory]
    [InlineData(CartItem.MinQuantity - 1)]
    [InlineData(CartItem.MaxQuantity + 1)]
    public async Task AddCartItem_WithQuantityOutOfRange_IsInvalid(int quantity)
    {
        var product = await TestData.ProductAsync();

        var response = await Client().PostAsJsonAsync("/api/cart/items", new { productId = product.Id, quantity });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.GetProperty("errors").TryGetProperty("Quantity", out _));
    }

    // Passes the DTO validation and fails in Identity (ValidationException): same shape as the automatic validation
    [Fact]
    public async Task RegisterCustomer_WithDuplicatedEmail_ReturnsErrorsByField()
    {
        await TestData.CustomerAsync(email: "customer@test.com");

        var response = await Client(null).PostAsJsonAsync("/api/auth/register/customer", new { name = "Test Customer", email = "customer@test.com", password = TestData.DefaultPassword });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.NotEmpty(body.GetProperty("errors").GetProperty("Email").EnumerateArray());
    }

    [Fact]
    public async Task GetProduct_WithNonNumericId_IsNotFound()
    {
        var response = await Client(null).GetAsync("/api/products/abc");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
