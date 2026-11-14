using Ecommerce.Infra.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace Ecommerce.IntegrationTests.Fixtures;

// Base for the service tests: joins the shared database collection and gives each test a clean state
[Collection(DatabaseCollection.Name)]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    // Fixed so dates written by the services are predictable
    protected static readonly DateTimeOffset Now = new(2026, 1, 15, 10, 30, 0, TimeSpan.Zero);

    private readonly DatabaseFixture _fixture;

    protected IntegrationTestBase(DatabaseFixture fixture)
    {
        _fixture = fixture;
        Services = new ServiceResolver(fixture);
        TestData = new TestData(Services);
    }

    // Services under test, resolved as a given user (Services.Get<ProductService>(sellerId: ...))
    protected ServiceResolver Services { get; }

    // Scenario building with valid defaults, used in the arrange and in the act
    protected TestData TestData { get; }

    // Lets a test move the clock (e.g. Time.Advance)
    protected FakeTimeProvider Time => _fixture.Time;

    // xUnit calls this before every test: each one starts from an empty database and the same clock,
    // without depending on what the previous test left behind
    public async Task InitializeAsync()
    {
        await _fixture.ResetAsync();

        _fixture.Time.SetUtcNow(Now);
    }

    // xUnit calls this after every test: releases the scopes opened during it
    public async Task DisposeAsync()
    {
        await Services.DisposeAsync();
    }

    // Only for effects no read operation exposes; reads from a fresh scope, never from the entities tracked by the service
    protected async Task<T> QueryAsync<T>(Func<AppDbContext, Task<T>> query)
    {
        await using var scope = _fixture.Services.CreateAsyncScope();

        return await query(scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }
}
