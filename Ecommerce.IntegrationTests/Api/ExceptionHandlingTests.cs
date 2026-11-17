using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ecommerce.Application.Services;
using Ecommerce.IntegrationTests.Fixtures;

namespace Ecommerce.IntegrationTests.Api;

// One case per mapped exception: the rules behind each one are covered by the service tests
public class ExceptionHandlingTests(DatabaseFixture fixture) : ApiTestBase(fixture)
{
    private const string ProblemJson = "application/problem+json";

    [Fact]
    public async Task CancelPurchase_WhenPaid_ReturnsCodeAndDetails()
    {
        var purchase = await TestData.PaidPurchaseAsync();

        var response = await Client().PostAsync($"/api/purchases/{purchase.Id}/cancel", null);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal(ProblemJson, response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("InvalidPurchaseStatus", body.GetProperty("code").GetString());
        Assert.Equal("Paid", body.GetProperty("details").GetProperty("currentStatus").GetString());
    }

    [Fact]
    public async Task GetOrder_WithUnknownId_IsNotFound()
    {
        var response = await Client().GetAsync("/api/orders/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(ProblemJson, response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task DeleteProduct_WhenProductHasOrders_IsConflict()
    {
        var product = await TestData.ProductAsync();
        await TestData.PurchaseAsync(products: [product]);

        var response = await Client(Seller).DeleteAsync($"/api/products/{product.Id}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(ProblemJson, response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task CancelOrder_WhenCancelledConcurrently_IsConcurrencyConflict()
    {
        var purchase = await TestData.PurchaseAsync();
        var orderId = purchase.Orders.Single().Id;
        BeforeNextWrite(() => Services.Get<OrderService>(Seller).CancelAsync(orderId));

        var response = await Client().PostAsync($"/api/orders/{orderId}/cancel", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(ProblemJson, response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Login_WithWrongPassword_IsUnauthorized()
    {
        await TestData.CustomerAsync(email: "customer@test.com");

        var response = await Client(null).PostAsJsonAsync("/api/auth/login", new { email = "customer@test.com", password = "wrong-password" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(ProblemJson, response.Content.Headers.ContentType?.MediaType);
    }

    // The product is deleted between the existence check and the insert: the foreign key violation is not mapped (500)
    [Fact]
    public async Task AddCartItem_WhenProductDeletedConcurrently_HidesExceptionDetails()
    {
        var product = await TestData.ProductAsync();
        BeforeNextWrite(() => Services.Get<ProductService>(Seller).DeleteAsync(product.Id));

        var response = await Client().PostAsJsonAsync("/api/cart/items", new { productId = product.Id, quantity = 1 });

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal(ProblemJson, response.Content.Headers.ContentType?.MediaType);
        Assert.DoesNotContain("Exception", await response.Content.ReadAsStringAsync());
    }
}
