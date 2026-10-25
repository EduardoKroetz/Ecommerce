using Ecommerce.Application.DTOs.Orders;
using Ecommerce.Domain.Enums;

namespace Ecommerce.Application.DTOs.Purchases;

public class PurchaseResponse
{
    public required int Id { get; set; }
    public required EPurchaseStatus Status { get; set; }
    public required EPaymentMethod PaymentMethod { get; set; }
    public required decimal OriginalTotalAmount { get; set; }
    public required decimal TotalAmount { get; set; }
    public required DateTime CreatedAt { get; set; }

    public required List<OrderResponse> Orders { get; set; }
}
