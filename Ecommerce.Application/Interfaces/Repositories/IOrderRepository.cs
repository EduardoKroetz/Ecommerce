using Ecommerce.Application.DTOs;
using Ecommerce.Application.DTOs.Orders;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interfaces.Repositories;

public interface IOrderRepository
{
    Task<PagedResult<GetOrder>> SearchByUserIdAsync(string userId, PaginationQuery query);
    Task<GetOrder?> GetDetailByIdAndUserIdAsync(int id, string userId, CancellationToken cancellationToken = default);

    Task<Order> AddAsync(Order order, CancellationToken cancellationToken = default);
    Task<Order> UpdateAsync(Order order, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdAndUserIdAsync(int id, string userId, CancellationToken cancellationToken = default);
}
