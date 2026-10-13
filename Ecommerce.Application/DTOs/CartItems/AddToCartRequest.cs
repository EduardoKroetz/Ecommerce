using System.ComponentModel.DataAnnotations;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.DTOs.CartItems;

public class AddToCartRequest
{
    public int ProductId { get; set; }

    [Range(1, CartItem.MAX_QUANTITY, ErrorMessage = "Quantity must be between {1} and {2}.")]
    public int Quantity { get; set; }
}
