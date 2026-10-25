namespace Ecommerce.Application.DTOs.Carts;

public class CartItemResponse
{
    public required int Id { get; set; }
    public required int ProductId { get; set; }
    public required string ProductName { get; set; }
    public required int Quantity { get; set; }
}
