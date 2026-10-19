using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interfaces.Repositories;

public interface ISellerRepository
{
    Task<Seller?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<Seller> AddAsync(Seller seller, CancellationToken cancellationToken = default);
}
