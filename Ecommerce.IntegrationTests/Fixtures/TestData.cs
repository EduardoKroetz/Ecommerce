using Ecommerce.Application.DTOs.Auth;
using Ecommerce.Application.DTOs.Carts;
using Ecommerce.Application.DTOs.ProductCategories;
using Ecommerce.Application.DTOs.Products;
using Ecommerce.Application.DTOs.Purchases;
using Ecommerce.Application.Services;
using Ecommerce.Domain.Enums;

namespace Ecommerce.IntegrationTests.Fixtures;

// Builds scenarios through the public services, with valid defaults: a test informs only what matters to it
public sealed class TestData(ServiceResolver services)
{
    private const string DefaultPassword = "Test@1234";

    // Keeps generated names and emails unique within a test
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

    public Task<RegisterCustomerResponse> CustomerAsync(string? name = null, string? email = null, string password = DefaultPassword)
    {
        var sequence = ++_sequence;

        return services.Get<AuthService>().RegisterCustomerAsync(new RegisterCustomerRequest
        {
            Name = name ?? $"Test Customer {sequence}",
            Email = email ?? $"customer{sequence}@test.com",
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

        return await services.Get<ProductService>(seller).CreateAsync(new CreateProductRequest
        {
            Name = name,
            Description = description,
            Price = price,
            StockBalance = stockBalance
        });
    }

    public Task<ProductCategoryResponse> CategoryAsync(string? name = null)
    {
        return services.Get<ProductCategoryService>().CreateAsync(new CreateProductCategoryRequest
        {
            Name = name ?? $"Test Category {++_sequence}"
        });
    }

    public async Task<CartResponse> CartItemAsync(RegisterCustomerResponse? customer = null, ProductResponse? product = null, int quantity = 1)
    {
        customer ??= await CustomerAsync();
        product ??= await ProductAsync();

        return await services.Get<CartService>(customer).AddToCartAsync(new AddCartItemRequest
        {
            ProductId = product.Id,
            Quantity = quantity
        });
    }

    // Goes through the real checkout: the products enter the customer's cart first
    public async Task<PurchaseResponse> PurchaseAsync(
        RegisterCustomerResponse? customer = null,
        IEnumerable<ProductResponse>? products = null,
        EPaymentMethod paymentMethod = EPaymentMethod.PIX)
    {
        customer ??= await CustomerAsync();
        products ??= [await ProductAsync()];

        foreach (var product in products)
            await CartItemAsync(customer, product);

        return await services.Get<PurchaseService>(customer).CreateAsync(new CreatePurchaseRequest
        {
            PaymentMethod = paymentMethod
        });
    }
}
