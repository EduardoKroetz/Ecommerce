using Ecommerce.Application.DTOs;
using Ecommerce.Application.DTOs.Purchases;
using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Domain.Entities;
using Ecommerce.Infra.Data;
using Ecommerce.Infra.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infra.Repositories;

public class PurchaseRepository(AppDbContext dbContext) : IPurchaseRepository
{
    public async Task<PagedResult<PurchaseResponse>> SearchForCustomerAsync(int customerId, PaginationQuery query, CancellationToken cancellationToken = default)
    {
        var purchaseQuery = dbContext.Purchases
            .AsNoTracking()
            .Where(p => p.CustomerId == customerId)
            .OrderByDescending(p => p.CreatedAt)
            .ThenBy(p => p.Id);

        return await purchaseQuery.ToPagedResultAsync(PurchaseResponse.MapExpression, query, cancellationToken);
    }

    public async Task<PurchaseResponse?> GetDetailByIdForCustomerAsync(int id, int customerId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Purchases
            .AsNoTracking()
            .Where(p => p.Id == id && p.CustomerId == customerId)
            .Select(PurchaseResponse.MapExpression)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Purchase?> GetByIdForCustomerAsync(int id, int customerId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Purchases
            .Include(p => p.Orders)
            .ThenInclude(o => o.Items)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(p => p.Id == id && p.CustomerId == customerId, cancellationToken);
    }

    public async Task<Purchase> AddAsync(Purchase purchase, CancellationToken cancellationToken = default)
    {
        await dbContext.Purchases.AddAsync(purchase, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return purchase;
    }

    public async Task<Purchase> UpdateAsync(Purchase purchase, CancellationToken cancellationToken = default)
    {
        dbContext.Purchases.Update(purchase);
        await dbContext.SaveChangesAsync(cancellationToken);
        return purchase;
    }
}
