using System.Linq.Expressions;
using Ecommerce.Application.DTOs.Carts;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Infra.Projections;

internal sealed class CartProjections
{
    public Expression<Func<CartItem, CartItemResponse>> ToItemResponse => cartItem => new CartItemResponse
    {
        Id = cartItem.Id,
        ProductId = cartItem.ProductId,
        ProductName = cartItem.Product.Name,
        Quantity = cartItem.Quantity
    };

    public Expression<Func<Cart, CartResponse>> ToResponse => cart => new CartResponse
    {
        Id = cart.Id,
        Items = cart.Items.AsQueryable().Select(ToItemResponse).ToList()
    };
}
