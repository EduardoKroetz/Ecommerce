using Ecommerce.Application.DTOs.CartItems;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interfaces.Repositories;

public interface ICartRepository
{
    Task<GetCart?> GetDetailByUserIdAsync(string userId);

    Task<Cart?> GetByUserIdAsync(string userId);

    Task<Cart> AddAsync(Cart cart, CancellationToken cancellationToken = default);
    Task<Cart> UpdateAsync(Cart cart, CancellationToken cancellationToken = default);
}
