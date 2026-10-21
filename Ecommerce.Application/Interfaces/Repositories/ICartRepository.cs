using Ecommerce.Application.DTOs.Carts;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interfaces.Repositories;

public interface ICartRepository
{
    Task<CartResponse?> GetDetailForCustomerAsync(int customerId);

    Task<Cart?> GetForCustomerAsync(int customerId);

    Task<Cart> AddAsync(Cart cart);
    Task<Cart> UpdateAsync(Cart cart);
}
