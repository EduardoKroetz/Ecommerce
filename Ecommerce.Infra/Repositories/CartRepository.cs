using Ecommerce.Application.DTOs.CartItems;
using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Domain.Entities;
using Ecommerce.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infra.Repositories;

public class CartRepository(AppDbContext dbContext) : ICartRepository
{
    public async Task<GetCart?> GetDetailByUserIdAsync(string userId)
    {
        return await dbContext.Carts
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .Select(GetCart.MapExpression)
            .FirstOrDefaultAsync();
    }

    public async Task<Cart?> GetByUserIdAsync(string userId)
    {
        return await dbContext.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task<Cart> AddAsync(Cart cart, CancellationToken cancellationToken = default)
    {
        await dbContext.Carts.AddAsync(cart, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return cart;
    }

    public async Task<Cart> UpdateAsync(Cart cart, CancellationToken cancellationToken = default)
    {
        dbContext.Carts.Update(cart);
        await dbContext.SaveChangesAsync(cancellationToken);
        return cart;
    }


}
