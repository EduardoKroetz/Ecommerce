using Ecommerce.Application.DTOs.Auth;
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

    // Where the clock starts in each test; the API tests start at the real time (ApiTestBase)
    protected virtual DateTimeOffset ClockStart => Now;

    private readonly DatabaseFixture _fixture;

    private readonly DefaultTestUsers _users = new();

    protected IntegrationTestBase(DatabaseFixture fixture)
    {
        _fixture = fixture;
        Services = new ServiceResolver(fixture, _users);
        TestData = new TestData(Services, _users);
    }

    // Services under test, resolved as the default customer or as a given user (Services.Get<ProductService>(Seller))
    protected ServiceResolver Services { get; }

    // Scenario building with valid defaults, used in the arrange and in the act
    protected TestData TestData { get; }

    // Lets a test move the clock (e.g. Time.Advance)
    protected FakeTimeProvider Time => _fixture.Time.Current;

    // Default users: Services.Get and TestData act as them when a test doesn't inform another.
    // Named explicitly when the role is what the test checks (Services.Get<OrderService>(Seller))
    protected RegisterCustomerResponse Customer => _users.Customer;

    protected RegisterSellerResponse Seller => _users.Seller;

    // xUnit calls this before every test: each one starts from an empty database, the same clock and its own
    // default users, without depending on what the previous test left behind
    public async Task InitializeAsync()
    {
        await _fixture.ResetAsync();

        _fixture.Time.Reset(ClockStart);

        _fixture.ConcurrentRequests.Reset();

        _users.Customer = await TestData.CustomerAsync();
        _users.Seller = await TestData.SellerAsync();
    }

    // xUnit calls this after every test: releases the scopes opened during it, and fails the test if the
    // concurrent request never ran (the operation threw before writing, so nothing was simulated)
    public async Task DisposeAsync()
    {
        await Services.DisposeAsync();

        var neverRan = _fixture.ConcurrentRequests.HasPendingRequest;
        _fixture.ConcurrentRequests.Reset();

        if (neverRan)
            throw new InvalidOperationException($"{nameof(BeforeNextWrite)} was called, but the operation under test never wrote.");
    }

    // Runs the request at the next write point of the operation under test, as if it committed concurrently
    // (ConcurrentRequestInterceptor)
    protected void BeforeNextWrite(Func<Task> concurrentRequest) =>
        _fixture.ConcurrentRequests.RunBeforeNextWrite(concurrentRequest);

    // Only for effects no read operation exposes; reads from a fresh scope, never from the entities tracked by the service
    protected async Task<T> QueryAsync<T>(Func<AppDbContext, Task<T>> query)
    {
        await using var scope = _fixture.Services.CreateAsyncScope();

        return await query(scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }
}
