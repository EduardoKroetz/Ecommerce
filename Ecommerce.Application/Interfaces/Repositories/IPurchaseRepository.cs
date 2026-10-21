using Ecommerce.Application.DTOs;
using Ecommerce.Application.DTOs.Purchases;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interfaces.Repositories;

public interface IPurchaseRepository
{
    Task<PagedResult<PurchaseResponse>> SearchForCustomerAsync(int customerId, PaginationQuery query);
    Task<PurchaseResponse?> GetDetailByIdForCustomerAsync(int id, int customerId);

    Task<Purchase?> GetByIdForCustomerAsync(int id, int customerId);
    Task<Purchase> AddAsync(Purchase purchase);
    Task<Purchase> UpdateAsync(Purchase purchase);
}
