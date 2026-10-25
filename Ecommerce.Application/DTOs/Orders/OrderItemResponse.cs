namespace Ecommerce.Application.DTOs.Orders;

public class OrderItemResponse
{
    public required int Id { get; set; }
    public required int ProductId { get; set; }
    public required string ProductName { get; set; }
    public required decimal UnitPrice { get; set; }
    public required int Quantity { get; set; }
    public required decimal TotalPrice { get; set; }
}
