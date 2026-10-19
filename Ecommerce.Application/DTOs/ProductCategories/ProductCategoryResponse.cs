using System.Linq.Expressions;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.DTOs.ProductCategories;

public class ProductCategoryResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public static Expression<Func<ProductCategory, ProductCategoryResponse>> MapExpression =>
        productCategory => new ProductCategoryResponse
        {
            Id = productCategory.Id,
            Name = productCategory.Name
        };

}
