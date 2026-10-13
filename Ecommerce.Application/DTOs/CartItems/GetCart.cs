using System.Linq.Expressions;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.DTOs.CartItems;

public class GetCart
{
    public required int Id { get; set; }
    public required List<GetCartItem> Items { get; set; }

    public static Expression<Func<Cart, GetCart>> MapExpression =>
        cart => new GetCart
        {
            Id = cart.Id,
            Items = cart.Items.AsQueryable().Select(GetCartItem.MapExpression).ToList()
        };

    private static readonly Func<Cart, GetCart> CompiledMap = MapExpression.Compile();

    public static GetCart Map(Cart cart) => CompiledMap(cart);
}
