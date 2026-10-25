using Ecommerce.Application.DTOs;
using Ecommerce.Application.DTOs.Purchases;
using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Domain.Entities;
using Ecommerce.Infra.Data;
using Ecommerce.Infra.Extensions;
using Ecommerce.Infra.Projections;
using Ecommerce.Infra.Services;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infra.Repositories;

internal class PurchaseRepository(AppDbContext dbContext, ICancellationTokenProvider cancellationTokenProvider, PurchaseProjections purchaseProjections) : IPurchaseRepository
{
    public async Task<PagedResult<PurchaseResponse>> SearchForCustomerAsync(int customerId, PaginationQuery query)
    {
        var purchaseQuery = dbContext.Purchases
            .AsNoTracking()
            .Where(p => p.CustomerId == customerId)
            .OrderByDescending(p => p.CreatedAt)
            .ThenBy(p => p.Id);

        return await purchaseQuery.ToPagedResultAsync(purchaseProjections.ToResponse, query, cancellationTokenProvider.Token);
    }

    public async Task<PurchaseResponse?> GetDetailByIdForCustomerAsync(int id, int customerId)
    {
        return await dbContext.Purchases
            .AsNoTracking()
            .Where(p => p.Id == id && p.CustomerId == customerId)
            .Select(purchaseProjections.ToResponse)
            .FirstOrDefaultAsync(cancellationTokenProvider.Token);
    }

    public async Task<Purchase?> GetByIdAsync(int id)
    {
        return await dbContext.Purchases
            .FirstOrDefaultAsync(p => p.Id == id, cancellationTokenProvider.Token);
    }

    public async Task<Purchase?> GetByIdForCustomerAsync(int id, int customerId)
    {
        return await dbContext.Purchases
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
