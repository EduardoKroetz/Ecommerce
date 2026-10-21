using Ecommerce.Application.DTOs;
using Ecommerce.Application.DTOs.Orders;
using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Domain.Entities;
using Ecommerce.Infra.Data;
using Ecommerce.Infra.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infra.Repositories;

public class OrderRepository(AppDbContext dbContext) : IOrderRepository
{
    public async Task<PagedResult<OrderResponse>> SearchForCustomerAsync(int customerId, PaginationQuery query, CancellationToken cancellationToken = default)
    {
        var orderQuery = dbContext.Orders
            .AsNoTracking()
            .Where(o => o.Purchase.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ThenBy(o => o.Id);

        return await orderQuery.ToPagedResultAsync(OrderResponse.MapExpression, query, cancellationToken);
    }

    public async Task<PagedResult<OrderResponse>> SearchForSellerAsync(int sellerId, PaginationQuery query, CancellationToken cancellationToken = default)
    {
        var orderQuery = dbContext.Orders
            .AsNoTracking()
            .Where(o => o.SellerId == sellerId)
            .OrderByDescending(o => o.CreatedAt)
            .ThenBy(o => o.Id);

        return await orderQuery.ToPagedResultAsync(OrderResponse.MapExpression, query, cancellationToken);
    }

    public async Task<OrderResponse?> GetDetailByIdForCustomerOrSellerAsync(int id, int customerId, int? sellerId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .Where(o => o.Id == id && (o.Purchase.CustomerId == customerId || o.SellerId == sellerId))
            .Select(OrderResponse.MapExpression)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Order?> GetByIdForCustomerAsync(int id, int customerId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Orders
            .FirstOrDefaultAsync(o => o.Id == id && o.Purchase.CustomerId == customerId, cancellationToken);
    }

    public async Task<Order?> GetByIdForSellerAsync(int id, int sellerId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Orders
            .FirstOrDefaultAsync(o => o.Id == id && o.SellerId == sellerId, cancellationToken);
    }

    public async Task<Order?> GetByIdForCustomerOrSellerAsync(int id, int customerId, int? sellerId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Orders
            .Include(o => o.Items)
            .ThenInclude(oi => oi.Product)
            .Include(o => o.Purchase)
            .ThenInclude(p => p.Orders)
            .FirstOrDefaultAsync(o => o.Id == id && (o.Purchase.CustomerId == customerId || o.SellerId == sellerId), cancellationToken);
    }

    public async Task<Order> UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        dbContext.Orders.Update(order);
        await dbContext.SaveChangesAsync(cancellationToken);
        return order;
    }
}
