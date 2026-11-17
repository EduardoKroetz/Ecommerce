using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ecommerce.Application.Services;
using Ecommerce.Domain.Enums;
using Ecommerce.IntegrationTests.Fixtures;

namespace Ecommerce.IntegrationTests.Api;

public class SerializationTests(DatabaseFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public async Task GetPurchase_ReturnsEnumsAsStrings()
    {
        var purchase = await TestData.PurchaseAsync(paymentMethod: EPaymentMethod.PIX);

        var response = await Client().GetAsync($"/api/purchases/{purchase.Id}");

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Pending", body.GetProperty("status").GetString());
        Assert.Equal("PIX", body.GetProperty("paymentMethod").GetString());
    }

    [Fact]
    public async Task CreatePurchase_WithPaymentMethodAsString_Persists()
    {
        await TestData.CartItemAsync();

        var response = await Client().PostAsJsonAsync("/api/purchases", new { paymentMethod = "Boleto" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var id = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();
        var purchase = await Services.Get<PurchaseService>().GetByIdAsync(id);
        Assert.Equal(EPaymentMethod.Boleto, purchase.PaymentMethod);
    }
}
