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
    public async Task<PagedResult<GetPurchase>> SearchByUserIdAsync(string userId, PaginationQuery query)
    {
        var purchaseQuery = dbContext.Purchases
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ThenBy(p => p.Id);

        return await purchaseQuery.ToPagedResultAsync(GetPurchase.MapExpression, query);
    }

    public async Task<GetPurchase?> GetDetailByIdAndUserIdAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Purchases
            .AsNoTracking()
            .Where(p => p.Id == id && p.UserId == userId)
            .Select(GetPurchase.MapExpression)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Purchase?> GetByIdAndUserIdAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Purchases
            .Include(p => p.Orders)
            .ThenInclude(o => o.Items)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId, cancellationToken);
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
