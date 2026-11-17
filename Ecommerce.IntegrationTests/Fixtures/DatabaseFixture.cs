using Ecommerce.Application.Extensions;
using Ecommerce.Application.Interfaces.Infra;
using Ecommerce.Infra.Data;
using Ecommerce.Infra.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Respawn;
using Respawn.Graph;
using Testcontainers.PostgreSql;

namespace Ecommerce.IntegrationTests.Fixtures;

// One Postgres container, one service provider and, for the API tests, one host for the whole suite (ADR 0021)
public class DatabaseFixture : IAsyncLifetime
{
    // Same major version as production (Neon)
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18").Build();

    // Built once from the schema; each reset only runs the delete statements it prepared
    private Respawner _respawner = null!;

    // Root provider: tests resolve services only through scopes (IntegrationTestBase.GetService)
    public ServiceProvider Services { get; private set; } = null!;

    // Shared clock that replaces TimeProvider, so tests control the current date
    public TestTimeProvider Time { get; } = new();

    // Attached to every DbContext; a test sets the request through IntegrationTestBase.BeforeNextWrite
    public ConcurrentRequestInterceptor ConcurrentRequests { get; } = new();

    private ApiFactory? _api;

    // Created on first use, so only the API tests pay for starting the host
    public ApiFactory Api => _api ??= new ApiFactory(this);

    // Test-only configuration, the same for the services under test and the API host
    public IReadOnlyDictionary<string, string?> Settings => new Dictionary<string, string?>
    {
        ["ConnectionStrings:DefaultConnection"] = _container.GetConnectionString(),
        ["Jwt:Key"] = TestJwt.Key,
        ["Jwt:Issuer"] = TestJwt.Issuer,
        ["Jwt:Audience"] = TestJwt.Audience,
        ["Jwt:ExpiryInMinutes"] = TestJwt.ExpiryInMinutes.ToString()
    };

    // xUnit calls this once, before the first test of the collection
    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        Services = BuildServices();

        // The real migrations, so the schema is the same as production
        using (var scope = Services.CreateScope())
            await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();

        await using var connection = new NpgsqlConnection(_container.GetConnectionString());
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            // Only test data is deleted: the migration history stays
            TablesToIgnore = [new Table("__EFMigrationsHistory")]
        });
    }

    // Deletes the rows of every table and keeps the schema; called before each test
    public async Task ResetAsync()
    {
        await using var connection = new NpgsqlConnection(_container.GetConnectionString());
        await connection.OpenAsync();

        await _respawner.ResetAsync(connection);
    }

    // xUnit calls this once, after the last test: stops and removes the container
    public async Task DisposeAsync()
    {
        if (_api is not null)
            await _api.DisposeAsync();

        await Services.DisposeAsync();
        await _container.DisposeAsync();
    }

    // Same registrations as Program.cs; only the ambient inputs (current user, clock) are replaced
    private ServiceProvider BuildServices()
    {
        // AuthService issues tokens, so the JWT settings must exist
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(Settings)
            .Build();

        var services = new ServiceCollection();

        // Provided by the web host in production, registered by hand here
        services.AddLogging();
        services.AddSingleton<IConfiguration>(configuration);

        services.AddInfrastructure(configuration);
        services.AddApplication();

        // The stub is resolved as ICurrentUserProvider in the same scope, so GetService sets the user before the service reads it
        services.AddScoped<TestCurrentUserProvider>();
        services.Replace(ServiceDescriptor.Scoped<ICurrentUserProvider>(sp => sp.GetRequiredService<TestCurrentUserProvider>()));

        AddTestOverrides(services);

        // Fails at startup if a service has a missing dependency, instead of in the middle of a test
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
    }

    // Shared with the API host (ApiFactory): clock, password hashing and the concurrent request interceptor
    public void AddTestOverrides(IServiceCollection services)
    {
        services.Replace(ServiceDescriptor.Singleton<TimeProvider>(Time));

        // The hash strength isn't under test, and the default 100,000 iterations made each registration
        // cost ~58 ms, two of them in every test (the default users)
        services.Configure<PasswordHasherOptions>(options => options.IterationCount = 1);

        // Added to the options registered by AddInfrastructure, without touching production code
        services.ConfigureDbContext<AppDbContext>((_, options) => options.AddInterceptors(ConcurrentRequests));
    }
}
