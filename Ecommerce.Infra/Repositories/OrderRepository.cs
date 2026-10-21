using Ecommerce.Application.DTOs;
using Ecommerce.Application.DTOs.Orders;
using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Domain.Entities;
using Ecommerce.Infra.Data;
using Ecommerce.Infra.Services;
using Ecommerce.Infra.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infra.Repositories;

public class OrderRepository(AppDbContext dbContext, ICancellationTokenProvider cancellationTokenProvider) : IOrderRepository
{
    public async Task<PagedResult<OrderResponse>> SearchForCustomerAsync(int customerId, PaginationQuery query)
    {
        var orderQuery = dbContext.Orders
            .AsNoTracking()
            .Where(o => o.Purchase.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ThenBy(o => o.Id);

        return await orderQuery.ToPagedResultAsync(OrderResponse.MapExpression, query, cancellationTokenProvider.Token);
    }

    public async Task<PagedResult<OrderResponse>> SearchForSellerAsync(int sellerId, PaginationQuery query)
    {
        var orderQuery = dbContext.Orders
            .AsNoTracking()
            .Where(o => o.SellerId == sellerId)
            .OrderByDescending(o => o.CreatedAt)
            .ThenBy(o => o.Id);

        return await orderQuery.ToPagedResultAsync(OrderResponse.MapExpression, query, cancellationTokenProvider.Token);
    }

    public async Task<OrderResponse?> GetDetailByIdForCustomerOrSellerAsync(int id, int customerId, int? sellerId)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .Where(o => o.Id == id && (o.Purchase.CustomerId == customerId || o.SellerId == sellerId))
            .Select(OrderResponse.MapExpression)
            .FirstOrDefaultAsync(cancellationTokenProvider.Token);
    }

    public async Task<Order?> GetByIdForCustomerAsync(int id, int customerId)
    {
        return await dbContext.Orders
            .FirstOrDefaultAsync(o => o.Id == id && o.Purchase.CustomerId == customerId, cancellationTokenProvider.Token);
    }

    public async Task<Order?> GetByIdForSellerAsync(int id, int sellerId)
    {
        return await dbContext.Orders
            .FirstOrDefaultAsync(o => o.Id == id && o.SellerId == sellerId, cancellationTokenProvider.Token);
    }

    public async Task<Order?> GetByIdForCustomerOrSellerAsync(int id, int customerId, int? sellerId)
    {
        return await dbContext.Orders
            .Include(o => o.Items)
            .ThenInclude(oi => oi.Product)
            .Include(o => o.Purchase)
            .ThenInclude(p => p.Orders)
            .FirstOrDefaultAsync(o => o.Id == id && (o.Purchase.CustomerId == customerId || o.SellerId == sellerId), cancellationTokenProvider.Token);
    }

    public async Task<Order> UpdateAsync(Order order)
    {
        dbContext.Orders.Update(order);
        await dbContext.SaveChangesAsync();
        return order;
    }
}
