using Ecommerce.Application.DTOs.Carts;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interfaces.Repositories;

public interface ICartRepository
{
    Task<CartResponse?> GetDetailForCustomerAsync(int customerId);

    Task<Cart?> GetForCustomerAsync(int customerId);

    void Add(Cart cart);
}
