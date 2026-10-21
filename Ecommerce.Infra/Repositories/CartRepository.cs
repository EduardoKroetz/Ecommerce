using Ecommerce.Application.DTOs.Carts;
using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Domain.Entities;
using Ecommerce.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infra.Repositories;

public class CartRepository(AppDbContext dbContext) : ICartRepository
{
    public async Task<CartResponse?> GetDetailForCustomerAsync(int customerId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Carts
            .AsNoTracking()
            .Where(c => c.CustomerId == customerId)
            .Select(CartResponse.MapExpression)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Cart?> GetForCustomerAsync(int customerId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);
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
