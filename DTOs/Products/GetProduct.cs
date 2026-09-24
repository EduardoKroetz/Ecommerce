using System.Linq.Expressions;
using Ecommerce.DTOs.ProductsCategories;
using Ecommerce.Models;

namespace Ecommerce.DTOs.Products;

public class GetProduct
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required decimal Price { get; set; }
    public int StockBalance { get; set; }
    public required DateTime CreatedAt { get; set; }

    public List<GetProductCategory> Categories { get; set; } = [];

    public static Expression<Func<Product, GetProduct>> MapExpression =>
        product => new GetProduct
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockBalance = product.StockBalance,
            CreatedAt = product.CreatedAt,
            Categories = product.Categories.AsQueryable().Select(GetProductCategory.MapExpression).ToList()
        };
}
