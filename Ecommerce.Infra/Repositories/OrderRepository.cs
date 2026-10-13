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
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ThenBy(o => o.Id);

        return await orderQuery.ToPagedResultAsync(GetOrder.MapExpression, query);
    }

    public async Task<Order?> GetByIdAndUserIdAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Orders
            .Include(o => o.Items)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId, cancellationToken);
    }

    public async Task<Order> AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        await dbContext.Orders.AddAsync(order, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return order;
    }

    public async Task<GetOrder?> GetDetailByIdAndUserIdAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId && o.Id == id)
            .Select(GetOrder.MapExpression)
            .FirstOrDefaultAsync();
    }

    public async Task<Order> UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        dbContext.Orders.Update(order);
        await dbContext.SaveChangesAsync(cancellationToken);
        return order;
    }
}
