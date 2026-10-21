using Ecommerce.Application.DTOs.Carts;
using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Domain.Entities;
using Ecommerce.Infra.Data;
using Ecommerce.Infra.Services;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infra.Repositories;

public class CartRepository(AppDbContext dbContext, ICancellationTokenProvider cancellationTokenProvider) : ICartRepository
{
    public async Task<CartResponse?> GetDetailForCustomerAsync(int customerId)
    {
        return await dbContext.Carts
            .AsNoTracking()
            .Where(c => c.CustomerId == customerId)
            .Select(CartResponse.MapExpression)
            .FirstOrDefaultAsync(cancellationTokenProvider.Token);
    }

    public async Task<Cart?> GetForCustomerAsync(int customerId)
    {
        return await dbContext.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationTokenProvider.Token);
    }

    public async Task<Cart> AddAsync(Cart cart)
    {
        await dbContext.Carts.AddAsync(cart);
        await dbContext.SaveChangesAsync();
        return cart;
    }

    public async Task<Cart> UpdateAsync(Cart cart)
    {
        dbContext.Carts.Update(cart);
        await dbContext.SaveChangesAsync();
        return cart;
    }


}
