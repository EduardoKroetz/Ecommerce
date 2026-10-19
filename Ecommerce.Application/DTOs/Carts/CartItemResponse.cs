using System.Linq.Expressions;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.DTOs.Carts;

public class CartItemResponse
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public required string ProductName { get; set; }
    public int Quantity { get; set; }

    public static Expression<Func<CartItem, CartItemResponse>> MapExpression =>
        cartItem => new CartItemResponse
        {
            Id = cartItem.Id,
            ProductId = cartItem.ProductId,
            ProductName = cartItem.Product.Name,
            Quantity = cartItem.Quantity
        };
}
