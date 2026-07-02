using AcademicGateway.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Respawn;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Xunit;

namespace IntegrationTests.Infrastructure;

/// <summary>
/// A specialized WebApplicationFactory that provisions an isolated PostgreSQL testing context,
/// runs database migrations safely across parallel execution pipelines exactly once,
/// and sets up high-performance mock authentication overlays.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private static readonly SemaphoreSlim DbMigrationLock = new(1, 1);
    private static bool _isDatabaseMigrated;

    private string _connectionString = string.Empty;
    private Respawner? _respawner;

    /// <summary>
    /// Thread-safe initialization gateway ensuring migrations are applied exactly once globally
    /// before configuring instance-level row resetting adapters.
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        _connectionString = configuration.GetConnectionString("TestConnection")
            ?? throw new InvalidOperationException("The 'TestConnection' connection string was not found in User Secrets configuration.");

        // --- GLOBAL LOCK: Prevents parallel xUnit collections from racing migrations ---
        await DbMigrationLock.WaitAsync();
        try
        {
            if (!_isDatabaseMigrated)
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Execute schema migration exactly once across the entire test process lifecycle
                await context.Database.MigrateAsync();
                _isDatabaseMigrated = true;
            }
        }
        finally
        {
            DbMigrationLock.Release();
        }

        // --- INSTANCE SETUP: Isolated per collection factory instance ---
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            TablesToIgnore = ["__EFMigrationsHistory"]
        });
    }

    /// <summary>
    /// Fast row-truncation mechanism invoked between isolated test runs to guarantee data sanitization
    /// without dropping or re-creating structural database tables.
    /// </summary>
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
            config.AddUserSecrets<CustomWebApplicationFactory>(optional: true);
            config.AddEnvironmentVariables();
        });

        builder.ConfigureServices((context, services) =>
        {
            // 1. Unregister any existing application database options descriptors
            var dbDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (dbDescriptor != null)
            {
                services.Remove(dbDescriptor);
            }

            var connString = context.Configuration.GetConnectionString("TestConnection");

            // 2. Map DbContext cleanly targeting our tracking target test database connection
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connString)
                       .UseSnakeCaseNamingConvention());

            // 3. Intercept perimeter authentication pipelines and swap in mock header evaluation engines
            services.Configure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = "TestAuthScheme";
                options.DefaultChallengeScheme = "TestAuthScheme";
            });

            services.AddAuthentication("TestAuthScheme")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestAuthScheme", _ => { });
        });
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
    }
}

/// <summary>
/// A high-performance mock authentication handler that extracts claims dynamically 
/// from custom execution headers supplied by our integration test clients.
/// </summary>
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Intercept request stream to check for customized automated testing identity contexts
        if (!Request.Headers.TryGetValue("X-Test-UserId", out var userIdValues) ||
            string.IsNullOrEmpty(userIdValues.ToString()))
        {
            return Task.FromResult(AuthenticateResult.Fail("No test security context provided. Request is processed as anonymous."));
        }

        var userId = userIdValues.ToString();
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };

        // Append role classification rules if specific target perimeters are evaluated
        if (Request.Headers.TryGetValue("X-Test-Role", out var roleValues) && !string.IsNullOrEmpty(roleValues.ToString()))
        {
            claims.Add(new Claim(ClaimTypes.Role, roleValues.ToString()));
        }

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestAuthScheme");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}