using Ecommerce.Application.DTOs;
using Ecommerce.Application.DTOs.Orders;
using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Domain.Entities;
using Ecommerce.Infra.Data;
using Ecommerce.Infra.Extensions;
using Ecommerce.Infra.Projections;
using Ecommerce.Infra.Services;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infra.Repositories;

internal class OrderRepository(AppDbContext dbContext, ICancellationTokenProvider cancellationTokenProvider, OrderProjections orderProjections) : IOrderRepository
{
    public async Task<PagedResult<OrderResponse>> SearchForCustomerAsync(int customerId, PaginationQuery query)
    {
        var orderQuery = ForCustomer(dbContext.Orders.AsNoTracking(), customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ThenBy(o => o.Id);

        return await orderQuery.ToPagedResultAsync(orderProjections.ToResponse, query, cancellationTokenProvider.Token);
    }

    public async Task<PagedResult<OrderResponse>> SearchForSellerAsync(int sellerId, PaginationQuery query)
    {
        var orderQuery = dbContext.Orders
            .AsNoTracking()
            .Where(o => o.SellerId == sellerId)
            .OrderByDescending(o => o.CreatedAt)
            .ThenBy(o => o.Id);

        return await orderQuery.ToPagedResultAsync(orderProjections.ToResponse, query, cancellationTokenProvider.Token);
    }

    public async Task<OrderResponse?> GetDetailByIdForCustomerOrSellerAsync(int id, int customerId, int? sellerId)
    {
        return await ForCustomerOrSeller(dbContext.Orders.AsNoTracking(), customerId, sellerId)
            .Where(o => o.Id == id)
            .Select(orderProjections.ToResponse)
            .FirstOrDefaultAsync(cancellationTokenProvider.Token);
    }

    public async Task<Order?> GetByIdForCustomerAsync(int id, int customerId)
    {
        return await ForCustomer(dbContext.Orders, customerId)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationTokenProvider.Token);
    }

    public async Task<Order?> GetByIdForSellerAsync(int id, int sellerId)
    {
        return await dbContext.Orders
            .FirstOrDefaultAsync(o => o.Id == id && o.SellerId == sellerId, cancellationTokenProvider.Token);
    }

    public async Task<Order?> GetByIdForCustomerOrSellerAsync(int id, int customerId, int? sellerId)
    {
        return await ForCustomerOrSeller(dbContext.Orders.Include(o => o.Items), customerId, sellerId)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationTokenProvider.Token);
    }

    public async Task<List<Order>> GetListByPurchaseIdAsync(int purchaseId)
    {
        return await dbContext.Orders
            .Include(o => o.Items)
            .Where(o => o.PurchaseId == purchaseId)
            .ToListAsync(cancellationTokenProvider.Token);
    }

    public async Task AddRangeAsync(IEnumerable<Order> orders)
    {
        await dbContext.Orders.AddRangeAsync(orders);
        await dbContext.SaveChangesAsync();
    }

    public async Task<Order> UpdateAsync(Order order)
    {
        dbContext.Orders.Update(order);
        await dbContext.SaveChangesAsync();
        return order;
    }

    public async Task UpdateRangeAsync(IEnumerable<Order> orders)
    {
        dbContext.Orders.UpdateRange(orders);
        await dbContext.SaveChangesAsync();
    }

    // The buyer lives on the purchase: resolved through the internal navigation, available to Infra queries only
    private IQueryable<Order> ForCustomer(IQueryable<Order> orders, int customerId) =>
        orders.Where(o => o.Purchase.CustomerId == customerId);

    private IQueryable<Order> ForCustomerOrSeller(IQueryable<Order> orders, int customerId, int? sellerId) =>
        orders.Where(o => o.SellerId == sellerId || o.Purchase.CustomerId == customerId);
}
