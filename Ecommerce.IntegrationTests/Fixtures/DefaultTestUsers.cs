using Ecommerce.Application.DTOs.Auth;

namespace Ecommerce.IntegrationTests.Fixtures;

// Default customer and seller of the current test, created before each one (IntegrationTestBase.InitializeAsync).
// Services.Get and TestData use them when a test doesn't inform another user.
// Two different people: the seller is also a customer, but not the default one.
public sealed class DefaultTestUsers
{
    public RegisterCustomerResponse Customer { get; set; } = null!;

    public RegisterSellerResponse Seller { get; set; } = null!;
}
