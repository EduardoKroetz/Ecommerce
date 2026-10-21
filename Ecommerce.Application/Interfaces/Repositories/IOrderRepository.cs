using Ecommerce.Application.DTOs;
using Ecommerce.Application.DTOs.Orders;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interfaces.Repositories;

public interface IOrderRepository
{
    Task<PagedResult<OrderResponse>> SearchForCustomerAsync(int customerId, PaginationQuery query, CancellationToken cancellationToken = default);
    Task<PagedResult<OrderResponse>> SearchForSellerAsync(int sellerId, PaginationQuery query, CancellationToken cancellationToken = default);
    Task<OrderResponse?> GetDetailByIdForCustomerOrSellerAsync(int id, int customerId, int? sellerId, CancellationToken cancellationToken = default);

    Task<Order?> GetByIdForCustomerAsync(int id, int customerId, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdForSellerAsync(int id, int sellerId, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdForCustomerOrSellerAsync(int id, int customerId, int? sellerId, CancellationToken cancellationToken = default);
    Task<Order> UpdateAsync(Order order, CancellationToken cancellationToken = default);
}
