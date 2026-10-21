using Ecommerce.Application.DTOs.Carts;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interfaces.Repositories;

public interface ICartRepository
{
    Task<CartResponse?> GetDetailForCustomerAsync(int customerId, CancellationToken cancellationToken = default);

    Task<Cart?> GetForCustomerAsync(int customerId, CancellationToken cancellationToken = default);

    Task<Cart> AddAsync(Cart cart, CancellationToken cancellationToken = default);
    Task<Cart> UpdateAsync(Cart cart, CancellationToken cancellationToken = default);
}
