using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Domain.Entities;
using Ecommerce.Infra.Data;
using Ecommerce.Infra.Services;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infra.Repositories;

public class CustomerRepository(AppDbContext dbContext, ICancellationTokenProvider cancellationTokenProvider) : ICustomerRepository
{
    public async Task<Customer?> GetByUserIdAsync(string userId)
    {
        return await dbContext.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationTokenProvider.Token);
    }

    public async Task<Customer> AddAsync(Customer customer)
    {
        await dbContext.Customers.AddAsync(customer);
        await dbContext.SaveChangesAsync();
        return customer;
    }
}
