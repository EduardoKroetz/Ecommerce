using Ecommerce.Domain.Enums;

namespace Ecommerce.Application.DTOs.Orders;

public class OrderResponse
{
    public required int Id { get; set; }
    public required int PurchaseId { get; set; }
    public required int SellerId { get; set; }
    public required EOrderStatus Status { get; set; }
    public required decimal TotalAmount { get; set; }
    public required DateTime CreatedAt { get; set; }

    public required List<OrderItemResponse> Items { get; set; }
}
