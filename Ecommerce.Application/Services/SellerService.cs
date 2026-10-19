using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Services;

public class SellerService(ISellerRepository sellerRepository)
{
    public async Task<Seller> CreateAsync(string userId)
    {
        var seller = new Seller
        {
            UserId = userId
        };

        return await sellerRepository.AddAsync(seller);
    }
}
