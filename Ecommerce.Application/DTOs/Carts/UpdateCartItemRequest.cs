using System.ComponentModel.DataAnnotations;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.DTOs.Carts;

public class UpdateCartItemRequest
{
    [Range(CartItem.MinQuantity, CartItem.MaxQuantity, ErrorMessage = "Quantity must be between {1} and {2}.")]
    public int Quantity { get; set; }
}
