using AcademicGateway.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Respawn.Graph;
using Xunit;

namespace AcademicGateway.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private string _connectionString = string.Empty;
    private Respawner? _respawner;

    public async ValueTask InitializeAsync()
    {
        // 1. Accessing 'Services' boots the host and triggers ConfigureWebHost
        using var scope = Services.CreateScope();

        // 2. Safely extract the connection string from the initialized configuration engine
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        _connectionString = configuration.GetConnectionString("TestConnection")
            ?? throw new InvalidOperationException("The 'TestConnection' connection string was not found in User Secrets.");

        // 3. Automatically build or update the database schema using your migrations
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        // 4. Initialize Respawn to cache the local database structure
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            TablesToIgnore = ["__EFMigrationsHistory"]
        });
    }

    public async Task ResetDatabaseAsync()
    {
        if (_respawner != null && !string.IsNullOrEmpty(_connectionString))
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await _respawner.ResetAsync(connection);
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Inject this test project's User Secrets and environment variables into the host configuration context
            config.AddUserSecrets<CustomWebApplicationFactory>(optional: true);
            config.AddEnvironmentVariables();
        });

        builder.ConfigureServices((context, services) =>
        {
            // Intercept and remove the application's production database registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Extract the connection string safely loaded from User Secrets above
            var connString = context.Configuration.GetConnectionString("TestConnection");

            // Bind the app context to use our local isolated test database string
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connString)
                       .UseSnakeCaseNamingConvention());
        });
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
    }
}