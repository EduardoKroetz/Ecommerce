using Ecommerce.Application.Extensions;
using Ecommerce.Application.Interfaces.Infra;
using Ecommerce.Infra.Data;
using Ecommerce.Infra.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Respawn;
using Respawn.Graph;
using Testcontainers.PostgreSql;

namespace Ecommerce.IntegrationTests.Fixtures;

// One Postgres container and one service provider for the whole suite (ADR 0021)
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

    // xUnit calls this once, before the first test of the collection
    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        Services = BuildServices(_container.GetConnectionString());

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
        await Services.DisposeAsync();
        await _container.DisposeAsync();
    }

    // Same registrations as Program.cs; only the ambient inputs (current user, clock) are replaced
    private ServiceProvider BuildServices(string connectionString)
    {
        // Test-only values: AuthService issues tokens, so the JWT settings must exist
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = connectionString,
                ["Jwt:Key"] = "integration-tests-signing-key-not-a-secret-0123456789",
                ["Jwt:Issuer"] = "Ecommerce.IntegrationTests",
                ["Jwt:Audience"] = "Ecommerce.IntegrationTests",
                ["Jwt:ExpiryInMinutes"] = "60"
            })
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
        services.Replace(ServiceDescriptor.Singleton<TimeProvider>(Time));

        // Fails at startup if a service has a missing dependency, instead of in the middle of a test
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
    }
}
