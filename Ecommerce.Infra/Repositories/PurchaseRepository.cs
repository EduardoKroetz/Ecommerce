using Ecommerce.Application.DTOs;
using Ecommerce.Application.DTOs.Purchases;
using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Domain.Entities;
using Ecommerce.Infra.Data;
using Ecommerce.Infra.Services;
using Ecommerce.Infra.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infra.Repositories;

public class PurchaseRepository(AppDbContext dbContext, ICancellationTokenProvider cancellationTokenProvider) : IPurchaseRepository
{
    public async Task<PagedResult<PurchaseResponse>> SearchForCustomerAsync(int customerId, PaginationQuery query)
    {
        var purchaseQuery = dbContext.Purchases
            .AsNoTracking()
            .Where(p => p.CustomerId == customerId)
            .OrderByDescending(p => p.CreatedAt)
            .ThenBy(p => p.Id);

        return await purchaseQuery.ToPagedResultAsync(PurchaseResponse.MapExpression, query, cancellationTokenProvider.Token);
    }

    public async Task<PurchaseResponse?> GetDetailByIdForCustomerAsync(int id, int customerId)
    {
        return await dbContext.Purchases
            .AsNoTracking()
            .Where(p => p.Id == id && p.CustomerId == customerId)
            .Select(PurchaseResponse.MapExpression)
            .FirstOrDefaultAsync(cancellationTokenProvider.Token);
    }

    public async Task<Purchase?> GetByIdForCustomerAsync(int id, int customerId)
    {
        return await dbContext.Purchases
            .Include(p => p.Orders)
            .ThenInclude(o => o.Items)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(p => p.Id == id && p.CustomerId == customerId, cancellationTokenProvider.Token);
    }

    public async Task<Purchase> AddAsync(Purchase purchase)
    {
        await dbContext.Purchases.AddAsync(purchase);
        await dbContext.SaveChangesAsync();
        return purchase;
    }

    public async Task<Purchase> UpdateAsync(Purchase purchase)
    {
        dbContext.Purchases.Update(purchase);
        await dbContext.SaveChangesAsync();
        return purchase;
    }
}
