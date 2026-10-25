using System.Linq.Expressions;
using Ecommerce.Application.DTOs.Purchases;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Infra.Projections;

internal sealed class PurchaseProjections(OrderProjections orderProjections)
{
    public Expression<Func<Purchase, PurchaseResponse>> ToResponse => purchase => new PurchaseResponse
    {
        Id = purchase.Id,
        Status = purchase.Status,
        PaymentMethod = purchase.PaymentMethod,
        OriginalTotalAmount = purchase.OriginalTotalAmount,
        TotalAmount = purchase.TotalAmount,
        CreatedAt = purchase.CreatedAt,
        Orders = purchase.Orders.AsQueryable().Select(orderProjections.ToResponse).ToList()
    };
}
