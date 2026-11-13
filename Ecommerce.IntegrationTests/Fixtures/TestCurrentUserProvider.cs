using Ecommerce.Application.Interfaces.Infra;

namespace Ecommerce.IntegrationTests.Fixtures;

// Stands in for the claims of the authenticated request: the test sets who is calling
public class TestCurrentUserProvider : ICurrentUserProvider
{
    public string UserId { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public int? SellerId { get; set; }
}
