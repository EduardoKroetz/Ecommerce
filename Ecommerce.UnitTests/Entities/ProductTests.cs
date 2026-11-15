using Ecommerce.Domain.Exceptions;

namespace Ecommerce.UnitTests.Entities;

public class ProductTests
{
    [Fact]
    public void AddCategory_WhenAlreadyLinked_KeepsSingleLink()
    {
        var product = ProductTestData.Create();
        var category = new ProductCategory { Name = "Test Category" }.WithId(1);
        product.AddCategory(category);

        product.AddCategory(category);

        Assert.Equal(1, Assert.Single(product.Categories).Id);
    }

    [Fact]
    public void RemoveCategory_WhenNotLinked_IsRejected()
    {
        var product = ProductTestData.Create();
        product.AddCategory(new ProductCategory { Name = "Test Category" }.WithId(1));

        var exception = Assert.Throws<DomainException>(() => product.RemoveCategory(2));
        Assert.Equal(EDomainExceptionCode.ProductCategoryNotAssociated, exception.Code);
        Assert.Equal(1, Assert.Single(product.Categories).Id);
    }
}
