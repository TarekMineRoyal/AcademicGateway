using AcademicGateway.Infrastructure.Persistence;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AcademicGateway.IntegrationTests;

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

    public async ValueTask InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}