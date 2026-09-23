namespace Ecommerce.Models;

public class Product
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required decimal Price { get; set; }
    public int StockBalance { get; set; }
    public required DateTime CreatedAt { get; set; }

    public List<ProductCategory> Categories { get; set; } = [];
}
