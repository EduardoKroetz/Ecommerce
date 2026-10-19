using System.Linq.Expressions;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;

namespace Ecommerce.Application.DTOs.Purchases;

public class GetPurchase
{
    public int Id { get; set; }
    public EPurchaseStatus Status { get; set; }
    public EPaymentMethod PaymentMethod { get; set; }
    public decimal OriginalTotalAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<GetPurchaseOrder> Orders { get; set; } = null!;

    public static Expression<Func<Purchase, GetPurchase>> MapExpression => purchase => new GetPurchase
    {
        Id = purchase.Id,
        Status = purchase.Status,
        PaymentMethod = purchase.PaymentMethod,
        OriginalTotalAmount = purchase.OriginalTotalAmount,
        TotalAmount = purchase.TotalAmount,
        CreatedAt = purchase.CreatedAt,
        Orders = purchase.Orders.AsQueryable().Select(GetPurchaseOrder.MapExpression).ToList()
    };

    private static readonly Func<Purchase, GetPurchase> CompiledMap = MapExpression.Compile();

    public static GetPurchase Map(Purchase purchase) => CompiledMap(purchase);
}

public class GetPurchaseOrder
{
    public int Id { get; set; }
    public int SellerId { get; set; }
    public EOrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<GetPurchaseOrderItem> Items { get; set; } = null!;

    public static Expression<Func<Order, GetPurchaseOrder>> MapExpression => order => new GetPurchaseOrder
    {
        Id = order.Id,
        SellerId = order.SellerId,
        Status = order.Status,
        TotalAmount = order.TotalAmount,
        CreatedAt = order.CreatedAt,
        Items = order.Items.AsQueryable().Select(GetPurchaseOrderItem.MapExpression).ToList()
    };
}

public class GetPurchaseOrderItem
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }

    public static Expression<Func<OrderItem, GetPurchaseOrderItem>> MapExpression => orderItem => new GetPurchaseOrderItem
    {
        Id = orderItem.Id,
        ProductId = orderItem.ProductId,
        ProductName = orderItem.ProductName,
        UnitPrice = orderItem.UnitPrice,
        Quantity = orderItem.Quantity,
        TotalPrice = orderItem.TotalPrice
    };
}
