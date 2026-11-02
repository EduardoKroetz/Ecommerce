using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interfaces.Repositories;

public interface ISellerRepository
{
    Task<Seller?> GetByUserIdAsync(string userId);
    void Add(Seller seller);
}
