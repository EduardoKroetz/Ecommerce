using Ecommerce.Application.DTOs.Products;
using Ecommerce.Application.Services;
using Ecommerce.Domain.Exceptions;
using Ecommerce.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.IntegrationTests.Services;

public class ProductServiceTests(DatabaseFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Create_PersistsProductForCurrentSeller()
    {
        var created = await TestData.ProductAsync(Seller, name: "Test Product", price: 10.0m, stockBalance: 5);

        var product = await Services.Get<ProductService>().GetByIdAsync(created.Id);
        Assert.Equal("Test Product", product.Name);
        Assert.Equal(10.0m, product.Price);
        Assert.Equal(5, product.StockBalance);
        Assert.Equal(Now.UtcDateTime, product.CreatedAt);

        var sellerId = await QueryAsync(db => db.Products.Where(p => p.Id == created.Id).Select(p => p.SellerId).SingleAsync());
        Assert.Equal(Seller.SellerId, sellerId);
    }

    [Fact]
    public async Task Create_WithoutSeller_IsUnauthorized()
    {
        var service = Services.Get<ProductService>(Customer);

        await Assert.ThrowsAsync<UnauthorizedException>(() => service.CreateAsync(new CreateProductRequest { Name = "Test Product", Price = 10.0m, StockBalance = 5 }));

        var products = await Services.Get<ProductService>().SearchAsync(new SearchProductsQuery());
        Assert.Equal(0, products.TotalCount);
    }

    [Fact]
    public async Task GetById_WithUnknownId_IsNotFound()
    {
        var service = Services.Get<ProductService>();

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(999999));
    }

    [Fact]
    public async Task Search_WithName_ReturnsProductsContainingNameIgnoringCase()
    {
        var notebook = await TestData.ProductAsync(name: "Notebook Dell");
        await TestData.ProductAsync(name: "Mouse Logitech");

        var result = await Services.Get<ProductService>().SearchAsync(new SearchProductsQuery { Name = "note" });

        Assert.Equal(notebook.Id, Assert.Single(result.Data).Id);
    }

    [Fact]
    public async Task Search_WithCategories_ReturnsProductsInAnyOfThem()
    {
        var (first, second, third) = (await TestData.ProductAsync(), await TestData.ProductAsync(), await TestData.ProductAsync());
        var (firstCategory, secondCategory, thirdCategory) = (await TestData.CategoryAsync(), await TestData.CategoryAsync(), await TestData.CategoryAsync());
        var sellerService = Services.Get<ProductService>(Seller);
        await sellerService.AddCategoryAsync(first.Id, firstCategory.Id);
        await sellerService.AddCategoryAsync(second.Id, secondCategory.Id);
        await sellerService.AddCategoryAsync(third.Id, thirdCategory.Id);

        var result = await Services.Get<ProductService>().SearchAsync(new SearchProductsQuery { Categories = [firstCategory.Id, secondCategory.Id] });

        Assert.Equal([first.Id, second.Id], result.Data.Select(p => p.Id).Order());
    }

    [Fact]
    public async Task Search_WithPriceRange_IncludesBounds()
    {
        var cheap = await TestData.ProductAsync(price: 10.0m);
        var middle = await TestData.ProductAsync(price: 20.0m);
        await TestData.ProductAsync(price: 30.0m);

        var result = await Services.Get<ProductService>().SearchAsync(new SearchProductsQuery { StartPrice = 10.0m, EndPrice = 20.0m });

        Assert.Equal([cheap.Id, middle.Id], result.Data.Select(p => p.Id).Order());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("unknown")]
    public async Task Search_WithoutKnownSort_OrdersByNewestFirst(string? sort)
    {
        var oldest = await TestData.ProductAsync();
        Time.Advance(TimeSpan.FromMinutes(1));
        var newest = await TestData.ProductAsync();

        var result = await Services.Get<ProductService>().SearchAsync(new SearchProductsQuery { Sort = sort });

        Assert.Equal([newest.Id, oldest.Id], result.Data.Select(p => p.Id));
    }

    [Theory]
    [InlineData(SearchProductsQuery.SortByNameAsc, "Apple,Banana,Cherry")]
    [InlineData(SearchProductsQuery.SortByPriceAsc, "Banana,Cherry,Apple")]
    [InlineData(SearchProductsQuery.SortByPriceDesc, "Apple,Cherry,Banana")]
    [InlineData(SearchProductsQuery.SortByCreatedAtAsc, "Cherry,Apple,Banana")]
    public async Task Search_WithSort_OrdersAccordingly(string sort, string expectedNames)
    {
        await TestData.ProductAsync(name: "Cherry", price: 20.0m);
        Time.Advance(TimeSpan.FromMinutes(1));
        await TestData.ProductAsync(name: "Apple", price: 30.0m);
        Time.Advance(TimeSpan.FromMinutes(1));
        await TestData.ProductAsync(name: "Banana", price: 10.0m);

        var result = await Services.Get<ProductService>().SearchAsync(new SearchProductsQuery { Sort = sort });

        Assert.Equal(expectedNames.Split(','), result.Data.Select(p => p.Name));
    }

    [Fact]
    public async Task Search_WithTiedSortValues_OrdersById()
    {
        var first = await TestData.ProductAsync(price: 10.0m);
        var second = await TestData.ProductAsync(price: 10.0m);

        var result = await Services.Get<ProductService>().SearchAsync(new SearchProductsQuery { Sort = SearchProductsQuery.SortByPriceAsc });

        Assert.Equal([first.Id, second.Id], result.Data.Select(p => p.Id));
    }

    [Fact]
    public async Task Search_WithPagination_ReturnsRequestedPageAndTotals()
    {
        var products = new List<ProductResponse>();
        for (var i = 0; i < 5; i++)
        {
            products.Add(await TestData.ProductAsync());
            Time.Advance(TimeSpan.FromMinutes(1));
        }

        var result = await Services.Get<ProductService>().SearchAsync(new SearchProductsQuery { PageSize = 2, PageNumber = 2 });

        Assert.Equal([products[2].Id, products[1].Id], result.Data.Select(p => p.Id));
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public async Task Update_PersistsNameDescriptionAndPrice()
    {
        var created = await TestData.ProductAsync();

        await Services.Get<ProductService>(Seller).UpdateAsync(created.Id, new UpdateProductRequest { Name = "Updated Product", Description = "Updated description", Price = 25.0m });

        var product = await Services.Get<ProductService>().GetByIdAsync(created.Id);
        Assert.Equal("Updated Product", product.Name);
        Assert.Equal("Updated description", product.Description);
        Assert.Equal(25.0m, product.Price);
    }

    [Fact]
    public async Task Update_WithProductFromAnotherSeller_IsNotFound()
    {
        var created = await TestData.ProductAsync(name: "Test Product", price: 10.0m);
        var otherSeller = await TestData.SellerAsync();
        var service = Services.Get<ProductService>(otherSeller);

        await Assert.ThrowsAsync<NotFoundException>(() => service.UpdateAsync(created.Id, new UpdateProductRequest { Name = "Hijacked", Price = 1.0m }));

        var product = await Services.Get<ProductService>().GetByIdAsync(created.Id);
        Assert.Equal("Test Product", product.Name);
        Assert.Equal(10.0m, product.Price);
    }

    // The update writes only the changed columns, so it doesn't overwrite the stock with the value it read
    [Fact]
    public async Task Update_WithConcurrentStockChange_KeepsStock()
    {
        var created = await TestData.ProductAsync(stockBalance: 10);
        BeforeNextWrite(() => Services.Get<ProductService>(Seller).AdjustStockAsync(created.Id, new AdjustProductStockRequest { Quantity = 5 }));

        await Services.Get<ProductService>(Seller).UpdateAsync(created.Id, new UpdateProductRequest { Name = "Updated Product", Price = 25.0m });

        var product = await Services.Get<ProductService>().GetByIdAsync(created.Id);
        Assert.Equal(15, product.StockBalance);
        Assert.Equal(25.0m, product.Price);
    }

    [Theory]
    [InlineData(5, 15)]
    [InlineData(-4, 6)]
    [InlineData(-10, 0)]
    public async Task AdjustStock_AppliesQuantityToStock(int quantity, int expectedStock)
    {
        var created = await TestData.ProductAsync(stockBalance: 10);

        await Services.Get<ProductService>(Seller).AdjustStockAsync(created.Id, new AdjustProductStockRequest { Quantity = quantity });

        var product = await Services.Get<ProductService>().GetByIdAsync(created.Id);
        Assert.Equal(expectedStock, product.StockBalance);
    }

    [Fact]
    public async Task AdjustStock_WhenResultWouldBeNegative_IsRejected()
    {
        var created = await TestData.ProductAsync(stockBalance: 10);
        var service = Services.Get<ProductService>(Seller);

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.AdjustStockAsync(created.Id, new AdjustProductStockRequest { Quantity = -11 }));
        Assert.Equal(EDomainExceptionCode.InsufficientStock, exception.Code);

        var product = await Services.Get<ProductService>().GetByIdAsync(created.Id);
        Assert.Equal(10, product.StockBalance);
    }

    [Fact]
    public async Task AdjustStock_WithProductFromAnotherSeller_IsNotFound()
    {
        var created = await TestData.ProductAsync(stockBalance: 10);
        var otherSeller = await TestData.SellerAsync();
        var service = Services.Get<ProductService>(otherSeller);

        await Assert.ThrowsAsync<NotFoundException>(() => service.AdjustStockAsync(created.Id, new AdjustProductStockRequest { Quantity = 5 }));

        var product = await Services.Get<ProductService>().GetByIdAsync(created.Id);
        Assert.Equal(10, product.StockBalance);
    }

    [Fact]
    public async Task Delete_RemovesProduct()
    {
        var created = await TestData.ProductAsync();

        await Services.Get<ProductService>(Seller).DeleteAsync(created.Id);

        var service = Services.Get<ProductService>();
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(created.Id));
    }

    [Fact]
    public async Task Delete_WithProductFromAnotherSeller_IsNotFound()
    {
        var created = await TestData.ProductAsync();
        var otherSeller = await TestData.SellerAsync();
        var service = Services.Get<ProductService>(otherSeller);

        await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(created.Id));

        var product = await Services.Get<ProductService>().GetByIdAsync(created.Id);
        Assert.Equal(created.Id, product.Id);
    }

    [Fact]
    public async Task Delete_WhenProductHasOrders_IsConflict()
    {
        var created = await TestData.ProductAsync();
        await TestData.PurchaseAsync(products: [created]);
        var service = Services.Get<ProductService>(Seller);

        await Assert.ThrowsAsync<ConflictException>(() => service.DeleteAsync(created.Id));

        var product = await Services.Get<ProductService>().GetByIdAsync(created.Id);
        Assert.Equal(created.Id, product.Id);
    }

    [Fact]
    public async Task Delete_WhenProductIsInCart_RemovesCartItem()
    {
        var created = await TestData.ProductAsync();
        await TestData.CartItemAsync(product: created);

        await Services.Get<ProductService>(Seller).DeleteAsync(created.Id);

        var cart = await Services.Get<CartService>().GetCartDetailAsync();
        Assert.Empty(cart.Items);
    }

    [Fact]
    public async Task AddCategory_LinksCategoryToProduct()
    {
        var created = await TestData.ProductAsync();
        var category = await TestData.CategoryAsync();

        await Services.Get<ProductService>(Seller).AddCategoryAsync(created.Id, category.Id);

        var product = await Services.Get<ProductService>().GetByIdAsync(created.Id);
        Assert.Equal(category.Id, Assert.Single(product.Categories).Id);
    }

    [Fact]
    public async Task AddCategory_WithUnknownCategory_IsNotFound()
    {
        var created = await TestData.ProductAsync();
        var service = Services.Get<ProductService>(Seller);

        await Assert.ThrowsAsync<NotFoundException>(() => service.AddCategoryAsync(created.Id, 999999));
    }

    [Fact]
    public async Task AddCategory_WithProductFromAnotherSeller_IsNotFound()
    {
        var created = await TestData.ProductAsync();
        var category = await TestData.CategoryAsync();
        var otherSeller = await TestData.SellerAsync();
        var service = Services.Get<ProductService>(otherSeller);

        await Assert.ThrowsAsync<NotFoundException>(() => service.AddCategoryAsync(created.Id, category.Id));

        var product = await Services.Get<ProductService>().GetByIdAsync(created.Id);
        Assert.Empty(product.Categories);
    }

    [Fact]
    public async Task RemoveCategory_UnlinksCategory()
    {
        var created = await TestData.ProductAsync();
        var category = await TestData.CategoryAsync();
        await Services.Get<ProductService>(Seller).AddCategoryAsync(created.Id, category.Id);

        await Services.Get<ProductService>(Seller).RemoveCategoryAsync(created.Id, category.Id);

        var product = await Services.Get<ProductService>().GetByIdAsync(created.Id);
        Assert.Empty(product.Categories);
    }

    [Fact]
    public async Task RemoveCategory_WithUnknownCategory_IsNotFound()
    {
        var created = await TestData.ProductAsync();
        var service = Services.Get<ProductService>(Seller);

        await Assert.ThrowsAsync<NotFoundException>(() => service.RemoveCategoryAsync(created.Id, 999999));
    }

    [Fact]
    public async Task RemoveCategory_WithProductFromAnotherSeller_IsNotFound()
    {
        var created = await TestData.ProductAsync();
        var category = await TestData.CategoryAsync();
        await Services.Get<ProductService>(Seller).AddCategoryAsync(created.Id, category.Id);
        var otherSeller = await TestData.SellerAsync();
        var service = Services.Get<ProductService>(otherSeller);

        await Assert.ThrowsAsync<NotFoundException>(() => service.RemoveCategoryAsync(created.Id, category.Id));

        var product = await Services.Get<ProductService>().GetByIdAsync(created.Id);
        Assert.Equal(category.Id, Assert.Single(product.Categories).Id);
    }
}
