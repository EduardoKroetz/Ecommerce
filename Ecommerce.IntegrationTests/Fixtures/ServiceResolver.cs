using Ecommerce.Application.DTOs.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.IntegrationTests.Fixtures;

// Resolves services the way a request would: one scope per call, acting as the given user
public sealed class ServiceResolver(DatabaseFixture fixture) : IAsyncDisposable
{
    // Scopes stay alive until the test ends: disposing earlier would dispose the service's DbContext
    private readonly List<AsyncServiceScope> _scopes = [];

    public T Get<T>(int customerId = 0, int? sellerId = null, string userId = "") where T : notnull
    {
        var scope = fixture.Services.CreateAsyncScope();
        _scopes.Add(scope);

        var currentUser = scope.ServiceProvider.GetRequiredService<TestCurrentUserProvider>();
        currentUser.UserId = userId;
        currentUser.CustomerId = customerId;
        currentUser.SellerId = sellerId;

        return scope.ServiceProvider.GetRequiredService<T>();
    }

    // Acting as a registered seller (who is also a customer)
    public T Get<T>(RegisterSellerResponse seller) where T : notnull =>
        Get<T>(customerId: seller.CustomerId, sellerId: seller.SellerId, userId: seller.UserId);

    // Acting as a registered customer
    public T Get<T>(RegisterCustomerResponse customer) where T : notnull =>
        Get<T>(customerId: customer.CustomerId, userId: customer.UserId);

    public async ValueTask DisposeAsync()
    {
        foreach (var scope in _scopes)
            await scope.DisposeAsync();

        _scopes.Clear();
    }
}
