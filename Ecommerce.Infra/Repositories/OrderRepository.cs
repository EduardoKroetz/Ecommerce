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
    public async Task<PagedResult<GetOrder>> SearchByUserIdAsync(string userId, PaginationQuery query)
    {
        var orderQuery = dbContext.Orders
            .AsNoTracking()
            .Where(o => o.Purchase.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ThenBy(o => o.Id);

        return await orderQuery.ToPagedResultAsync(GetOrder.MapExpression, query);
    }

    public async Task<PagedResult<GetOrder>> SearchBySellerIdAsync(int sellerId, PaginationQuery query)
    {
        var orderQuery = dbContext.Orders
            .AsNoTracking()
            .Where(o => o.SellerId == sellerId)
            .OrderByDescending(o => o.CreatedAt)
            .ThenBy(o => o.Id);

        return await orderQuery.ToPagedResultAsync(GetOrder.MapExpression, query);
    }

    public async Task<GetOrder?> GetDetailByIdAndUserIdOrSellerIdAsync(int id, string userId, int? sellerId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .Where(o => o.Id == id && (o.Purchase.UserId == userId || o.SellerId == sellerId))
            .Select(GetOrder.MapExpression)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Order?> GetByIdAndUserIdAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Orders
            .FirstOrDefaultAsync(o => o.Id == id && o.Purchase.UserId == userId, cancellationToken);
    }

    public async Task<Order?> GetByIdAndSellerIdAsync(int id, int sellerId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Orders
            .FirstOrDefaultAsync(o => o.Id == id && o.SellerId == sellerId, cancellationToken);
    }

    public async Task<Order?> GetByIdAndUserIdOrSellerIdAsync(int id, string userId, int? sellerId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Orders
            .Include(o => o.Items)
            .ThenInclude(oi => oi.Product)
            .Include(o => o.Purchase)
            .ThenInclude(p => p.Orders)
            .FirstOrDefaultAsync(o => o.Id == id && (o.Purchase.UserId == userId || o.SellerId == sellerId), cancellationToken);
    }

    public async Task<Order> UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        dbContext.Orders.Update(order);
        await dbContext.SaveChangesAsync(cancellationToken);
        return order;
    }
}
