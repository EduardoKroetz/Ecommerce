using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Services;

public class CustomerService(ICustomerRepository customerRepository)
{
    public async Task<Customer> CreateAsync(string userId)
    {
        var customer = new Customer
        {
            UserId = userId
        };

        return await customerRepository.AddAsync(customer);
    }
}
