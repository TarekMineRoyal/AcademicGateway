using AcademicGateway.Infrastructure.Identity;
using Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Respawn;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
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
        // Accessing 'Services' boots the web host and executes Program.cs top-to-bottom.
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

            // 1. Flush tables cleanly without destroying structure
            await _respawner.ResetAsync(connection);

            // 2. Re-apply lookup tables and seed constants needed by individual tests
            using var scope = Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            await ApplicationDbContextSeed.SeedDefaultUserAndDataAsync(userManager, roleManager, context);
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        var initialTestConfig = new ConfigurationBuilder()
            .AddUserSecrets<CustomWebApplicationFactory>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        var testConnectionString = initialTestConfig.GetConnectionString("TestConnection");

        // Prepare settings including dummy JWT configuration parameters for the Testing host context
        var testSettings = new Dictionary<string, string?>
        {
            { "JwtSettings:Secret", "this_is_a_very_long_mock_secret_key_for_testing_purposes_only_32_bytes_long!" },
            { "JwtSettings:Issuer", "AcademicGatewayApi" },
            { "JwtSettings:Audience", "AcademicGatewayUsers" },
            { "JwtSettings:ExpiryMinutes", "60" }
        };

        if (!string.IsNullOrEmpty(testConnectionString))
        {
            testSettings.Add("ConnectionStrings:DefaultConnection", testConnectionString);
            testSettings.Add("ConnectionStrings:TestConnection", testConnectionString);
        }

        var hostConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(testSettings)
            .Build();

        builder.UseConfiguration(hostConfig);

        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(testSettings);
            configBuilder.AddUserSecrets<CustomWebApplicationFactory>(optional: true);
            configBuilder.AddEnvironmentVariables();
        });

        builder.ConfigureServices((context, services) =>
        {
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
    // Corrected .NET 8 signature requirement mapping for options monitor resolution loops
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