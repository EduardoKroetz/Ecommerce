using Ecommerce.Domain.Enums;

namespace Ecommerce.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public required EOrderStatus Status { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required decimal TotalAmount { get; set; }

    public List<OrderItem> Items { get; set; } = [];
}
