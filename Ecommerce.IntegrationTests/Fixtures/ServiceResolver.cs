using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.IntegrationTests.Fixtures;

// Resolves services the way a request would: one scope per call, acting as the given user
public sealed class ServiceResolver(DatabaseFixture fixture, DefaultTestUsers users) : IAsyncDisposable
{
    // Scopes stay alive until the test ends: disposing earlier would dispose the service's DbContext
    private readonly List<AsyncServiceScope> _scopes = [];

    // Without a user, acts as the default customer, the same way TestData falls back to its defaults
    public T Get<T>() where T : notnull =>
        Get<T>(users.Customer ?? throw new InvalidOperationException("The default users are created in IntegrationTestBase.InitializeAsync."));

    // null acts as an anonymous request
    public T Get<T>(TestUser? user) where T : notnull
    {
        var scope = fixture.Services.CreateAsyncScope();
        _scopes.Add(scope);

        var currentUser = scope.ServiceProvider.GetRequiredService<TestCurrentUserProvider>();
        currentUser.UserId = user?.UserId ?? "";
        currentUser.CustomerId = user?.CustomerId ?? 0;
        currentUser.SellerId = user?.SellerId;

        return scope.ServiceProvider.GetRequiredService<T>();
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var scope in _scopes)
            await scope.DisposeAsync();

        _scopes.Clear();
    }
}
