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

namespace AcademicGateway.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private string _connectionString = string.Empty;
    private Respawner? _respawner;

    public async ValueTask InitializeAsync()
    {
        using var scope = Services.CreateScope();

        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        _connectionString = configuration.GetConnectionString("TestConnection")
            ?? throw new InvalidOperationException("The 'TestConnection' connection string was not found in User Secrets.");

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

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
            config.AddUserSecrets<CustomWebApplicationFactory>(optional: true);
            config.AddEnvironmentVariables();
        });

        builder.ConfigureServices((context, services) =>
        {
            // 1. Re-route database context to test database
            var dbDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (dbDescriptor != null)
            {
                services.Remove(dbDescriptor);
            }

            var connString = context.Configuration.GetConnectionString("TestConnection");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connString)
                       .UseSnakeCaseNamingConvention());

            // 2. Intercept and mock Authentication pipeline for integration tests
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
/// from HTTP headers supplied by our integration test clients.
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
        // Check if the inbound test request requested authorization claims
        if (!Request.Headers.TryGetValue("X-Test-UserId", out var userIdValues) ||
            string.IsNullOrEmpty(userIdValues.ToString()))
        {
            return Task.FromResult(AuthenticateResult.Fail("No test security context provided. Request is anonymous."));
        }

        var userId = userIdValues.ToString();
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };

        // Append the role claim if a role header is specified
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