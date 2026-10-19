using Ecommerce.Application.DTOs;
using Ecommerce.Application.DTOs.Purchases;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interfaces.Repositories;

public interface IPurchaseRepository
{
    Task<PagedResult<PurchaseResponse>> SearchByUserIdAsync(string userId, PaginationQuery query);
    Task<PurchaseResponse?> GetDetailByIdAndUserIdAsync(int id, string userId, CancellationToken cancellationToken = default);

    Task<Purchase?> GetByIdAndUserIdAsync(int id, string userId, CancellationToken cancellationToken = default);
    Task<Purchase> AddAsync(Purchase purchase, CancellationToken cancellationToken = default);
    Task<Purchase> UpdateAsync(Purchase purchase, CancellationToken cancellationToken = default);
}
