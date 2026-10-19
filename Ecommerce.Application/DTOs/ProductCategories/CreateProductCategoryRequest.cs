using System.ComponentModel.DataAnnotations;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.DTOs.ProductCategories;

public class CreateProductCategoryRequest
{
    [Required(ErrorMessage = "Product category name is required.")]
    [MaxLength(ProductCategory.NameMaxLength, ErrorMessage = "Product category name cannot exceed {1} characters.")]
    public required string Name { get; set; }
}
