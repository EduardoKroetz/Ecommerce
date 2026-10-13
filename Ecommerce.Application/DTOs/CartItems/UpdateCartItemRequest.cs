using System.ComponentModel.DataAnnotations;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.DTOs.CartItems;

public class UpdateCartItemRequest
{
    [Range(1, CartItem.MAX_QUANTITY, ErrorMessage = "Quantity must be between {1} and {2}.")]
    public int Quantity { get; set; }
}
