using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Domain.Entities;
using Ecommerce.Infra.Data;
using Ecommerce.Infra.Services;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infra.Repositories;

public class SellerRepository(AppDbContext dbContext, ICancellationTokenProvider cancellationTokenProvider) : ISellerRepository
{
    public async Task<Seller?> GetByUserIdAsync(string userId)
    {
        return await dbContext.Sellers
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationTokenProvider.Token);
    }

    public async Task<Seller> AddAsync(Seller seller)
    {
        await dbContext.Sellers.AddAsync(seller);
        await dbContext.SaveChangesAsync();
        return seller;
    }
}
