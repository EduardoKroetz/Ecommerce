using System.Linq.Expressions;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;

namespace Ecommerce.Application.DTOs.Orders;

public class OrderResponse
{
    public int Id { get; set; }
    public int PurchaseId { get; set; }
    public int SellerId { get; set; }
    public EOrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<OrderItemResponse> Items { get; set; } = null!;

    public static Expression<Func<Order, OrderResponse>> MapExpression => order => new OrderResponse
    {
        Id = order.Id,
        PurchaseId = order.PurchaseId,
        SellerId = order.SellerId,
        Status = order.Status,
        TotalAmount = order.TotalAmount,
        CreatedAt = order.CreatedAt,
        Items = order.Items.AsQueryable().Select(OrderItemResponse.MapExpression).ToList()
    };

    private static readonly Func<Order, OrderResponse> CompiledMap = MapExpression.Compile();

    public static OrderResponse Map(Order order) => CompiledMap(order);
}
