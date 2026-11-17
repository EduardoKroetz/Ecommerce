using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

namespace Ecommerce.IntegrationTests.Fixtures;

// The real API host (Program.cs) on the test database, for the tests of the HTTP edge (ADR 0021)
public sealed class ApiFactory(DatabaseFixture fixture) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // As deployed: e.g. a 500 must not expose exception details
        builder.UseEnvironment(Environments.Production);

        foreach (var (key, value) in fixture.Settings)
            builder.UseSetting(key, value);

        // Same test-only replacements as the services under test; the current user comes from the real JWT
        builder.ConfigureTestServices(fixture.AddTestOverrides);
    }
}
