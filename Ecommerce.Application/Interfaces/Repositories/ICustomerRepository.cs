using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interfaces.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> GetByUserIdAsync(string userId);
    void Add(Customer customer);
}
