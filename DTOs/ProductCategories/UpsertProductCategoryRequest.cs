using System.ComponentModel.DataAnnotations;

namespace Ecommerce.DTOs.ProductCategories;

public class UpsertProductCategoryRequest
{
    [Required(ErrorMessage = "Product category name is required.")]
    [MaxLength(100, ErrorMessage = "Product category name cannot exceed 100 characters.")]
    public required string Name { get; set; }
}
