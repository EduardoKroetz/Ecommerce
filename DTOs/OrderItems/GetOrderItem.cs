using System.Linq.Expressions;
using Ecommerce.Models;

namespace Ecommerce.DTOs.OrderItems;

public class GetOrderItem
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }

    public static Expression<Func<OrderItem, GetOrderItem>> MapExpression => orderItem => new GetOrderItem
    {
        Id = orderItem.Id,
        ProductId = orderItem.ProductId,
        ProductName = orderItem.ProductName,
        UnitPrice = orderItem.UnitPrice,
        Quantity = orderItem.Quantity,
        TotalPrice = orderItem.TotalPrice
    };
}
