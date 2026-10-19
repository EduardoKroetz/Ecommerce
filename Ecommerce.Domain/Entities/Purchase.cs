using Ecommerce.Domain.Enums;

namespace Ecommerce.Domain.Entities;

public class Purchase
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public required decimal OriginalTotalAmount { get; set; }
    public required decimal TotalAmount { get; set; }
    public required EPaymentMethod PaymentMethod { get; set; }
    public required EPurchaseStatus Status { get; set; }
    public required DateTime CreatedAt { get; set; }

    public List<Order> Orders { get; set; } = [];
}
