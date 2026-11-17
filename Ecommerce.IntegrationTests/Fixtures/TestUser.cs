using Ecommerce.Application.DTOs.Auth;

namespace Ecommerce.IntegrationTests.Fixtures;

// Who a service acts as. Registered customers and sellers convert to it, so tests pass them directly,
// and a single parameter type lets Services.Get<T>(null) mean an anonymous request.
// The token is the one issued at registration, used by the API tests (ApiTestBase.Client)
public sealed record TestUser(string UserId, int CustomerId, int? SellerId, string Token)
{
    public static implicit operator TestUser(RegisterCustomerResponse customer) =>
        new(customer.UserId, customer.CustomerId, null, customer.Token);

    public static implicit operator TestUser(RegisterSellerResponse seller) =>
        new(seller.UserId, seller.CustomerId, seller.SellerId, seller.Token);
}
