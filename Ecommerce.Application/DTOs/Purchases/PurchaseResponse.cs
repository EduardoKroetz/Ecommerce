using System.Linq.Expressions;
using Ecommerce.Application.DTOs.Orders;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;

namespace Ecommerce.Application.DTOs.Purchases;

public class PurchaseResponse
{
    public int Id { get; set; }
    public EPurchaseStatus Status { get; set; }
    public EPaymentMethod PaymentMethod { get; set; }
    public decimal OriginalTotalAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<OrderResponse> Orders { get; set; } = null!;

    public static Expression<Func<Purchase, PurchaseResponse>> MapExpression => purchase => new PurchaseResponse
    {
        Id = purchase.Id,
        Status = purchase.Status,
        PaymentMethod = purchase.PaymentMethod,
        OriginalTotalAmount = purchase.OriginalTotalAmount,
        TotalAmount = purchase.TotalAmount,
        CreatedAt = purchase.CreatedAt,
        Orders = purchase.Orders.AsQueryable().Select(OrderResponse.MapExpression).ToList()
    };

    private static readonly Func<Purchase, PurchaseResponse> CompiledMap = MapExpression.Compile();

    public static PurchaseResponse Map(Purchase purchase) => CompiledMap(purchase);
}
