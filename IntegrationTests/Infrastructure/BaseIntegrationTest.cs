using AcademicGateway.Infrastructure.Persistence;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Xunit;

namespace IntegrationTests.Infrastructure;

[Collection("SharedDatabase")]
public abstract class BaseIntegrationTest : IAsyncLifetime
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly CustomWebApplicationFactory _factory;

    protected BaseIntegrationTest(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();
    }

    protected async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        using var scope = _scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
        return await mediator.Send(request);
    }

    protected async Task SendAsync(IRequest request)
    {
        using var scope = _scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
        await mediator.Send(request);
    }

    protected async Task<TEntity?> FindAsync<TEntity>(params object[] keyValues) where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await context.Set<TEntity>().FindAsync(keyValues);
    }

    protected async Task AddAsync<TEntity>(TEntity entity) where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Set<TEntity>().AddAsync(entity);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Provisions an unauthenticated, anonymous HTTP client.
    /// </summary>
    protected HttpClient GetAnonymousClient()
    {
        return _factory.CreateClient();
    }

    /// <summary>
    /// Provisions an authenticated HTTP client bound strictly to the 'Student' role boundary.
    /// </summary>
    protected HttpClient GetStudentClient(string? userId = null)
    {
        var client = _factory.CreateClient();
        var resolvedUserId = userId ?? Guid.NewGuid().ToString();

        client.DefaultRequestHeaders.Add("X-Test-UserId", resolvedUserId);
        client.DefaultRequestHeaders.Add("X-Test-Role", "Student");

        return client;
    }

    /// <summary>
    /// Provisions an authenticated HTTP client bound strictly to the 'Provider' role boundary.
    /// </summary>
    protected HttpClient GetProviderClient(string? userId = null)
    {
        var client = _factory.CreateClient();
        var resolvedUserId = userId ?? Guid.NewGuid().ToString();

        client.DefaultRequestHeaders.Add("X-Test-UserId", resolvedUserId);
        client.DefaultRequestHeaders.Add("X-Test-Role", "Provider");

        return client;
    }

    /// <summary>
    /// Provisions an authenticated HTTP client bound strictly to the 'Reviewer' role boundary.
    /// </summary>
    protected HttpClient GetReviewerClient(string? userId = null)
    {
        var client = _factory.CreateClient();
        var resolvedUserId = userId ?? Guid.NewGuid().ToString();

        client.DefaultRequestHeaders.Add("X-Test-UserId", resolvedUserId);
        client.DefaultRequestHeaders.Add("X-Test-Role", "Reviewer");

        return client;
    }

    public async ValueTask InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}