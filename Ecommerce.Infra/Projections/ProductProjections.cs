using System.Linq.Expressions;
using Ecommerce.Application.DTOs.Products;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Infra.Projections;

internal sealed class ProductProjections(ProductCategoryProjections productCategoryProjections)
{
    public Expression<Func<Product, ProductResponse>> ToResponse => product => new ProductResponse
    {
        Id = product.Id,
        Name = product.Name,
        Description = product.Description,
        Price = product.Price,
        StockBalance = product.StockBalance,
        CreatedAt = product.CreatedAt,
        Categories = product.Categories.AsQueryable().Select(productCategoryProjections.ToResponse).ToList()
    };
}
