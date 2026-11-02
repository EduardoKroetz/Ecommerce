using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Services;

public class SellerService(ISellerRepository sellerRepository)
{
    // The id is only generated when the caller saves
    public Seller Create(string userId)
    {
        var seller = new Seller
        {
            UserId = userId
        };

        sellerRepository.Add(seller);

        return seller;
    }
}
