using Ecommerce.Application.DTOs.ProductCategories;

namespace Ecommerce.Application.DTOs.Products;

public class ProductResponse
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required string? Description { get; set; }
    public required decimal Price { get; set; }
    public required int StockBalance { get; set; }
    public required DateTime CreatedAt { get; set; }

    public required List<ProductCategoryResponse> Categories { get; set; }
}
