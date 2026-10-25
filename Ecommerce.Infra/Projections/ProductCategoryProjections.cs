using System.Linq.Expressions;
using Ecommerce.Application.DTOs.ProductCategories;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Infra.Projections;

internal sealed class ProductCategoryProjections
{
    public Expression<Func<ProductCategory, ProductCategoryResponse>> ToResponse => productCategory => new ProductCategoryResponse
    {
        Id = productCategory.Id,
        Name = productCategory.Name
    };
}
