using Ecommerce.Application.DTOs;
using Ecommerce.Application.DTOs.Orders;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interfaces.Repositories;

public interface IOrderRepository
{
    Task<PagedResult<OrderResponse>> SearchForCustomerAsync(int customerId, PaginationQuery query);
    Task<PagedResult<OrderResponse>> SearchForSellerAsync(int sellerId, PaginationQuery query);
    Task<OrderResponse?> GetDetailByIdForCustomerOrSellerAsync(int id, int customerId, int? sellerId);

    Task<Order?> GetByIdForCustomerAsync(int id, int customerId);
    Task<Order?> GetByIdForSellerAsync(int id, int sellerId);
    Task<Order?> GetByIdForCustomerOrSellerAsync(int id, int customerId, int? sellerId);
    Task<Order> UpdateAsync(Order order);
}
