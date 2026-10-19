using System.Linq.Expressions;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.DTOs.Carts;

public class CartResponse
{
    public required int Id { get; set; }
    public required List<CartItemResponse> Items { get; set; }

    public static Expression<Func<Cart, CartResponse>> MapExpression =>
        cart => new CartResponse
        {
            Id = cart.Id,
            Items = cart.Items.AsQueryable().Select(CartItemResponse.MapExpression).ToList()
        };

    private static readonly Func<Cart, CartResponse> CompiledMap = MapExpression.Compile();

    public static CartResponse Map(Cart cart) => CompiledMap(cart);
}
