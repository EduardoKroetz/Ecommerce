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
            Items = cart.Items.Select(GetCartItem.MapExpression.Compile()).ToList()
        };

    public static GetCart Map(Cart cart) => MapExpression.Compile().Invoke(cart);
}
