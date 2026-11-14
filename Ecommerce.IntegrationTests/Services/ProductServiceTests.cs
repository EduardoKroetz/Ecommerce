using Ecommerce.Application.Services;
using Ecommerce.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.IntegrationTests.Services;

public class ProductServiceTests(DatabaseFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Create_PersistsProductForCurrentSeller()
    {
        var seller = await TestData.SellerAsync();

        var created = await TestData.ProductAsync(seller, name: "Test Product", price: 10.0m, stockBalance: 5);

        var product = await Services.Get<ProductService>().GetByIdAsync(created.Id);
        Assert.Equal("Test Product", product.Name);
        Assert.Equal(10.0m, product.Price);
        Assert.Equal(5, product.StockBalance);
        Assert.Equal(Now.UtcDateTime, product.CreatedAt);

        var sellerId = await QueryAsync(db => db.Products.Where(p => p.Id == created.Id).Select(p => p.SellerId).SingleAsync());
        Assert.Equal(seller.SellerId, sellerId);
    }

    [Fact]
    public async Task Reset_StartsEachTestWithEmptyDatabase()
    {
        var productCount = await QueryAsync(db => db.Products.CountAsync());

        Assert.Equal(0, productCount);
    }
}
