using Ecommerce.Application.DTOs.Auth;
using Ecommerce.Application.DTOs.Carts;
using Ecommerce.Application.DTOs.ProductCategories;
using Ecommerce.Application.DTOs.Products;
using Ecommerce.Application.DTOs.Purchases;
using Ecommerce.Application.Services;
using Ecommerce.Domain.Enums;

namespace Ecommerce.IntegrationTests.Fixtures;

// Builds scenarios through the public services, with valid defaults: a test informs only what matters to it.
// Without a customer or seller, uses the default ones of the test (DefaultTestUsers).
public sealed class TestData(ServiceResolver services, DefaultTestUsers users)
{
    // Public so login tests can reuse the credentials of a registered user
    public const string DefaultPassword = "Test@1234";

    // Keeps generated names and emails unique within a test
    private int _sequence;

    public Task<RegisterSellerResponse> SellerAsync(string? name = null, string? email = null, string password = DefaultPassword)
    {
        var sequence = ++_sequence;

        // Registration is an anonymous request
        return services.Get<AuthService>(null).RegisterSellerAsync(new RegisterSellerRequest
        {
            Name = name ?? $"Test Seller {sequence}",
            Email = email ?? $"seller{sequence}@test.com",
            Password = password
        });
    }

    public Task<RegisterCustomerResponse> CustomerAsync(string? name = null, string? email = null, string password = DefaultPassword)
    {
        var sequence = ++_sequence;

        return services.Get<AuthService>(null).RegisterCustomerAsync(new RegisterCustomerRequest
        {
            Name = name ?? $"Test Customer {sequence}",
            Email = email ?? $"customer{sequence}@test.com",
            Password = password
        });
    }

    public async Task<ProductResponse> ProductAsync(
        RegisterSellerResponse? seller = null,
        string name = "Test Product",
        string? description = "Test description",
        decimal price = 10.0m,
        int stockBalance = 100)
    {
        seller ??= users.Seller;

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
        customer ??= users.Customer;
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
        customer ??= users.Customer;
        products ??= [await ProductAsync()];

        foreach (var product in products)
            await CartItemAsync(customer, product);

        return await services.Get<PurchaseService>(customer).CreateAsync(new CreatePurchaseRequest
        {
            PaymentMethod = paymentMethod
        });
    }

    // Read back after paying, so the orders come with their new status
    public async Task<PurchaseResponse> PaidPurchaseAsync(RegisterCustomerResponse? customer = null, IEnumerable<ProductResponse>? products = null)
    {
        customer ??= users.Customer;

        var purchase = await PurchaseAsync(customer, products);

        await services.Get<PurchaseService>(customer).PayAsync(purchase.Id);

        return await services.Get<PurchaseService>(customer).GetByIdAsync(purchase.Id);
    }
}
