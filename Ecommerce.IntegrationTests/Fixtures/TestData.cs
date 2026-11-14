using Ecommerce.Application.DTOs.Auth;
using Ecommerce.Application.DTOs.Products;
using Ecommerce.Application.Services;

namespace Ecommerce.IntegrationTests.Fixtures;

// Builds scenarios through the public services, with valid defaults: a test informs only what matters to it
public sealed class TestData(ServiceResolver services)
{
    private const string DefaultPassword = "Test@1234";

    // Keeps generated emails unique within a test
    private int _sequence;

    public Task<RegisterSellerResponse> SellerAsync(string? name = null, string? email = null, string password = DefaultPassword)
    {
        var sequence = ++_sequence;

        return services.Get<AuthService>().RegisterSellerAsync(new RegisterSellerRequest
        {
            Name = name ?? $"Test Seller {sequence}",
            Email = email ?? $"seller{sequence}@test.com",
            Password = password
        });
    }

    // Without a seller, creates one: the product always needs an owner
    public async Task<ProductResponse> ProductAsync(
        RegisterSellerResponse? seller = null,
        string name = "Test Product",
        string? description = "Test description",
        decimal price = 10.0m,
        int stockBalance = 100)
    {
        seller ??= await SellerAsync();

        return await services.Get<ProductService>(customerId: seller.CustomerId, sellerId: seller.SellerId, userId: seller.UserId)
            .CreateAsync(new CreateProductRequest
            {
                Name = name,
                Description = description,
                Price = price,
                StockBalance = stockBalance
            });
    }
}
