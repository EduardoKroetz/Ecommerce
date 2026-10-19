using Ecommerce.Domain.Enums;

namespace Ecommerce.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public int PurchaseId { get; set; }
    public required int SellerId { get; set; }
    public required EOrderStatus Status { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required decimal TotalAmount { get; set; }

    public Purchase Purchase { get; set; } = null!;
    public List<OrderItem> Items { get; set; } = [];
}
