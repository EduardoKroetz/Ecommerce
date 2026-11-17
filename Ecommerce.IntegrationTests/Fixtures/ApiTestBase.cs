using System.Net.Http.Headers;

namespace Ecommerce.IntegrationTests.Fixtures;

// Base for the tests of the HTTP edge: the real host and JWT, over the same database, TestData and default users
// as the service tests. Business rules stay in the service tests (ADR 0021)
public abstract class ApiTestBase(DatabaseFixture fixture) : IntegrationTestBase(fixture)
{
    // The JWT handler validates the lifetime against the real clock, so tokens are issued at the real time
    protected override DateTimeOffset ClockStart => DateTimeOffset.UtcNow;

    // Like Services.Get: without a user, the default customer; null is an anonymous request
    protected HttpClient Client() => Client(Customer);

    protected HttpClient Client(TestUser? user) => user is null ? fixture.Api.CreateClient() : ClientWithToken(user.Token);

    // For tokens that don't come from a registered user (TestTokens, login)
    protected HttpClient ClientWithToken(string token)
    {
        var client = fixture.Api.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return client;
    }
}
