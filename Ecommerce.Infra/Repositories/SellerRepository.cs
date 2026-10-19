using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Domain.Entities;
using Ecommerce.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infra.Repositories;

public class SellerRepository(AppDbContext dbContext) : ISellerRepository
{
    public async Task<Seller?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Sellers
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
    }

    public async Task<Seller> AddAsync(Seller seller, CancellationToken cancellationToken = default)
    {
        await dbContext.Sellers.AddAsync(seller, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return seller;
    }
}
