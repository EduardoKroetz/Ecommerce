using System.ComponentModel.DataAnnotations;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.DTOs.Products;

public class CreateProductRequest
{
    [Required(ErrorMessage = "Product name is required.")]
    [MaxLength(Product.NameMaxLength, ErrorMessage = "Product name cannot exceed {1} characters.")]
    public required string Name { get; set; }

    [MaxLength(Product.DescriptionMaxLength, ErrorMessage = "Product description cannot exceed {1} characters.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Product price is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Product price must be a positive value.")]
    public required decimal Price { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Stock balance must be a non-negative value.")]
    public int StockBalance { get; set; }
}
