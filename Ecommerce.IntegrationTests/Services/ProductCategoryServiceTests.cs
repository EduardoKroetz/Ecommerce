using Ecommerce.Application.DTOs;
using Ecommerce.Application.DTOs.ProductCategories;
using Ecommerce.Application.Services;
using Ecommerce.Domain.Exceptions;
using Ecommerce.IntegrationTests.Fixtures;

namespace Ecommerce.IntegrationTests.Services;

public class ProductCategoryServiceTests(DatabaseFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Create_PersistsCategory()
    {
        var created = await TestData.CategoryAsync(name: "Electronics");

        var category = await Services.Get<ProductCategoryService>().GetByIdAsync(created.Id);

        Assert.Equal("Electronics", category.Name);
    }

    [Fact]
    public async Task GetById_WithUnknownId_IsNotFound()
    {
        var service = Services.Get<ProductCategoryService>();

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(999999));
    }

    [Fact]
    public async Task Search_OrdersByName()
    {
        await TestData.CategoryAsync(name: "Toys");
        await TestData.CategoryAsync(name: "Books");
        await TestData.CategoryAsync(name: "Electronics");

        var result = await Services.Get<ProductCategoryService>().SearchAsync(new PaginationQuery());

        Assert.Equal(["Books", "Electronics", "Toys"], result.Data.Select(c => c.Name));
    }

    [Fact]
    public async Task Search_WithPagination_ReturnsRequestedPageAndTotals()
    {
        await TestData.CategoryAsync(name: "Books");
        await TestData.CategoryAsync(name: "Electronics");
        await TestData.CategoryAsync(name: "Toys");

        var result = await Services.Get<ProductCategoryService>().SearchAsync(new PaginationQuery { PageSize = 2, PageNumber = 2 });

        Assert.Equal("Toys", Assert.Single(result.Data).Name);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
    }

    [Fact]
    public async Task Update_PersistsName()
    {
        var created = await TestData.CategoryAsync(name: "Electronics");

        await Services.Get<ProductCategoryService>().UpdateAsync(created.Id, new UpdateProductCategoryRequest { Name = "Computers" });

        var category = await Services.Get<ProductCategoryService>().GetByIdAsync(created.Id);
        Assert.Equal("Computers", category.Name);
    }

    [Fact]
    public async Task Update_WithUnknownId_IsNotFound()
    {
        var service = Services.Get<ProductCategoryService>();

        await Assert.ThrowsAsync<NotFoundException>(() => service.UpdateAsync(999999, new UpdateProductCategoryRequest { Name = "Computers" }));
    }

    [Fact]
    public async Task Delete_RemovesCategory()
    {
        var created = await TestData.CategoryAsync();

        await Services.Get<ProductCategoryService>().DeleteAsync(created.Id);

        var service = Services.Get<ProductCategoryService>();
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(created.Id));
    }

    [Fact]
    public async Task Delete_WithUnknownId_IsNotFound()
    {
        var service = Services.Get<ProductCategoryService>();

        await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(999999));
    }

    [Fact]
    public async Task Delete_WhenLinkedToProduct_UnlinksFromProduct()
    {
        var product = await TestData.ProductAsync();
        var created = await TestData.CategoryAsync();
        await Services.Get<ProductService>(Seller).AddCategoryAsync(product.Id, created.Id);

        await Services.Get<ProductCategoryService>().DeleteAsync(created.Id);

        var linked = await Services.Get<ProductService>().GetByIdAsync(product.Id);
        Assert.Empty(linked.Categories);
    }
}
