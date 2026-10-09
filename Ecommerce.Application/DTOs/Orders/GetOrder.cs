using System.Linq.Expressions;
using Ecommerce.Application.DTOs.OrderItems;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;

namespace Ecommerce.Application.DTOs.Orders;

public class GetOrder
{
    public int Id { get; set; }
    public EOrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<GetOrderItem> Items { get; set; } = null!;

    public static Expression<Func<Order, GetOrder>> MapExpression => order => new GetOrder
    {
        Id = order.Id,
        Status = order.Status,
        TotalAmount = order.TotalAmount,
        CreatedAt = order.CreatedAt,
        Items = order.Items.AsQueryable().Select(GetOrderItem.MapExpression).ToList()
    };
}
