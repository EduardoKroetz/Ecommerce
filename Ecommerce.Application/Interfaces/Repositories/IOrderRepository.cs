using Ecommerce.Application.DTOs;
using Ecommerce.Application.DTOs.Orders;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interfaces.Repositories;

public interface IOrderRepository
{
    Task<PagedResult<OrderResponse>> SearchByUserIdAsync(string userId, PaginationQuery query);
    Task<PagedResult<OrderResponse>> SearchBySellerIdAsync(int sellerId, PaginationQuery query);
    Task<OrderResponse?> GetDetailByIdAndUserIdOrSellerIdAsync(int id, string userId, int? sellerId, CancellationToken cancellationToken = default);

    Task<Order?> GetByIdAndUserIdAsync(int id, string userId, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdAndSellerIdAsync(int id, int sellerId, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdAndUserIdOrSellerIdAsync(int id, string userId, int? sellerId, CancellationToken cancellationToken = default);
    Task<Order> UpdateAsync(Order order, CancellationToken cancellationToken = default);
}
