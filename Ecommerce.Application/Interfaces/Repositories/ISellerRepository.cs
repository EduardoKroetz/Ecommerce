using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interfaces.Repositories;

public interface ISellerRepository
{
    Task<Seller?> GetByUserIdAsync(string userId);
    Task<Seller> AddAsync(Seller seller);
}
