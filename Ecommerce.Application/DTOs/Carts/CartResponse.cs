namespace Ecommerce.Application.DTOs.Carts;

public class CartResponse
{
    public required int Id { get; set; }
    public required List<CartItemResponse> Items { get; set; }
}
