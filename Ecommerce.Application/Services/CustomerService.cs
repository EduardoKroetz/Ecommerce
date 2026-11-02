using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Services;

public class CustomerService(ICustomerRepository customerRepository)
{
    // The id is only generated when the caller saves
    public Customer Create(string userId)
    {
        var customer = new Customer
        {
            UserId = userId
        };

        customerRepository.Add(customer);

        return customer;
    }
}
