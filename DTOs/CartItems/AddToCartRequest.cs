using System.ComponentModel.DataAnnotations;

namespace Ecommerce.DTOs.CartItems;

public class AddToCartRequest
{
    public int ProductId { get; set; }

    [Range(1, 9999, ErrorMessage = "Quantity must be between {1} and {2}.")]
    public int Quantity { get; set; }
}
