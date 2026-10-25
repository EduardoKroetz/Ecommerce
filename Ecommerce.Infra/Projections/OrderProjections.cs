using System.Linq.Expressions;
using Ecommerce.Application.DTOs.Orders;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Infra.Projections;

internal sealed class OrderProjections
{
    public Expression<Func<OrderItem, OrderItemResponse>> ToItemResponse => orderItem => new OrderItemResponse
    {
        Id = orderItem.Id,
        ProductId = orderItem.ProductId,
        ProductName = orderItem.ProductName,
        UnitPrice = orderItem.UnitPrice,
        Quantity = orderItem.Quantity,
        TotalPrice = orderItem.TotalPrice
    };

    public Expression<Func<Order, OrderResponse>> ToResponse => order => new OrderResponse
    {
        Id = order.Id,
        PurchaseId = order.PurchaseId,
        SellerId = order.SellerId,
        Status = order.Status,
        TotalAmount = order.TotalAmount,
        CreatedAt = order.CreatedAt,
        Items = order.Items.AsQueryable().Select(ToItemResponse).ToList()
    };
}
