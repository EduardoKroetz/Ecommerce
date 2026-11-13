using Ecommerce.Infra.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace Ecommerce.IntegrationTests.Fixtures;

// Base for the service tests: joins the shared database collection and gives each test a clean state
[Collection(DatabaseCollection.Name)]
public abstract class IntegrationTestBase(DatabaseFixture fixture) : IAsyncLifetime
{
    // Fixed so dates written by the services are predictable
    protected static readonly DateTimeOffset Now = new(2026, 1, 15, 10, 30, 0, TimeSpan.Zero);

    // Scopes opened by GetService stay alive until the test ends: disposing earlier would dispose the service's DbContext
    private readonly List<AsyncServiceScope> _scopes = [];

    // Lets a test move the clock (e.g. Time.Advance)
    protected FakeTimeProvider Time => fixture.Time;

    // xUnit calls this before every test: each one starts from an empty database and the same clock,
    // without depending on what the previous test left behind
    public async Task InitializeAsync()
    {
        await fixture.ResetAsync();

        fixture.Time.SetUtcNow(Now);
    }

    // xUnit calls this after every test: releases the scopes opened during it
    public async Task DisposeAsync()
    {
        foreach (var scope in _scopes)
            await scope.DisposeAsync();
    }

    // Each call is a new scope, like a new request, acting as the given user
    protected T GetService<T>(int customerId = 0, int? sellerId = null, string userId = "") where T : notnull
    {
        var scope = fixture.Services.CreateAsyncScope();
        _scopes.Add(scope);

        var currentUser = scope.ServiceProvider.GetRequiredService<TestCurrentUserProvider>();
        currentUser.UserId = userId;
        currentUser.CustomerId = customerId;
        currentUser.SellerId = sellerId;

        return scope.ServiceProvider.GetRequiredService<T>();
    }

    // Assertions read from a fresh scope, never from the entities tracked by the service under test
    protected async Task<T> QueryAsync<T>(Func<AppDbContext, Task<T>> query)
    {
        await using var scope = fixture.Services.CreateAsyncScope();

        return await query(scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }
}
