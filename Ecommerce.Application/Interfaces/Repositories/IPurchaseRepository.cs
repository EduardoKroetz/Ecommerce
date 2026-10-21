using Ecommerce.Application.DTOs;
using Ecommerce.Application.DTOs.Purchases;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interfaces.Repositories;

public interface IPurchaseRepository
{
    Task<PagedResult<PurchaseResponse>> SearchForCustomerAsync(int customerId, PaginationQuery query, CancellationToken cancellationToken = default);
    Task<PurchaseResponse?> GetDetailByIdForCustomerAsync(int id, int customerId, CancellationToken cancellationToken = default);

    Task<Purchase?> GetByIdForCustomerAsync(int id, int customerId, CancellationToken cancellationToken = default);
    Task<Purchase> AddAsync(Purchase purchase, CancellationToken cancellationToken = default);
    Task<Purchase> UpdateAsync(Purchase purchase, CancellationToken cancellationToken = default);
}
