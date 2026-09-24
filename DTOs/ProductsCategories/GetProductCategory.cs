using System.Linq.Expressions;
using Ecommerce.Models;

namespace Ecommerce.DTOs.ProductsCategories;

public class GetProductCategory
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public static Expression<Func<ProductCategory, GetProductCategory>> MapExpression =>
        productCategory => new GetProductCategory
        {
            Id = productCategory.Id,
            Name = productCategory.Name
        };

}
