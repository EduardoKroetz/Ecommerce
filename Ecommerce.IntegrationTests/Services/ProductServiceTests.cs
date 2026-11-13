using Ecommerce.Application.DTOs.Auth;
using Ecommerce.Application.DTOs.Products;
using Ecommerce.Application.Services;
using Ecommerce.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.IntegrationTests.Services;

public class ProductServiceTests(DatabaseFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task CreateAsync_PersistsProductForCurrentSeller()
    {
        var seller = await GetService<AuthService>().RegisterSellerAsync(new RegisterSellerRequest
        {
            Name = "Test Seller",
            Email = "seller@test.com",
            Password = "Test@1234"
        });

        var created = await GetService<ProductService>(customerId: seller.CustomerId, sellerId: seller.SellerId).CreateAsync(new CreateProductRequest
        {
            Name = "Test Product",
            Description = "Test description",
            Price = 10.0m,
            StockBalance = 5
        });

        var product = await QueryAsync(db => db.Products.AsNoTracking().SingleAsync(p => p.Id == created.Id));
        Assert.Equal(seller.SellerId, product.SellerId);
        Assert.Equal("Test Product", product.Name);
        Assert.Equal(10.0m, product.Price);
        Assert.Equal(5, product.StockBalance);
        Assert.Equal(Now.UtcDateTime, product.CreatedAt);
    }

    [Fact]
    public async Task Reset_StartsEachTestWithEmptyDatabase()
    {
        var productCount = await QueryAsync(db => db.Products.CountAsync());

        Assert.Equal(0, productCount);
    }
}
