using System.ComponentModel.DataAnnotations;

namespace Ecommerce.DTOs.Products;

public class UpsertProductRequest
{
    [Required(ErrorMessage = "Product name is required.")]
    [MaxLength(100, ErrorMessage = "Product name cannot exceed 100 characters.")]
    public required string Name { get; set; }

    [MaxLength(500, ErrorMessage = "Product description cannot exceed 500 characters.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Product price is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Product price must be a positive value.")]
    public required decimal Price { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Stock balance must be a non-negative value.")]
    public int StockBalance { get; set; }
}
