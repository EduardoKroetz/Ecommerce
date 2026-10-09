using System.Linq.Expressions;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.DTOs.ProductCategories;

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
