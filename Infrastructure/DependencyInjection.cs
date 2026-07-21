using System;
using System.Net.Http;
using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Infrastructure.Identity;
using AcademicGateway.Infrastructure.Persistence.Context;
using AcademicGateway.Infrastructure.Persistence.Interceptors;
using AcademicGateway.Infrastructure.Services;
using AcademicGateway.Infrastructure.Services.AiMatchmaking;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AcademicGateway.Infrastructure;

/// <summary>
/// Centralized container configuration engine for the Infrastructure project layer.
/// Registers adapters, persistence definitions, and internal pipeline interceptors.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Configures core infrastructure requirements, binds database pipelines, and attaches tracking interceptors.
    /// </summary>
    /// <param name="services">The centralized service collection container registry.</param>
    /// <param name="configuration">The environment configuration dictionary provider.</param>
    /// <returns>The updated service collection mapping.</returns>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Fetch connection variables safely from configuration parameters
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Database Connection String 'DefaultConnection' was not found.");

        // 2. Register our newly decoupled domain event loop interceptor as a scoped service
        services.AddScoped<DispatchDomainEventsInterceptor>();

        // 3. Configure the DbContext with PostgreSQL and SnakeCase conventions
        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            var interceptor = sp.GetRequiredService<DispatchDomainEventsInterceptor>();

            options.UseNpgsql(connectionString)
                   .UseSnakeCaseNamingConvention()
                   .AddInterceptors(interceptor);
        });

        // 4. Register the Application Unit of Work Context proxy mapping contract
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        // 5. Register ASP.NET Core Identity Framework infrastructure
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.User.RequireUniqueEmail = true;
        })
        .AddRoles<IdentityRole<Guid>>()
        .AddEntityFrameworkStores<ApplicationDbContext>();

        // 6. Register Identity adapters and core cross-cutting utility tools
        services.AddHttpContextAccessor(); // Allows resolving the HttpContext from outside of controllers
        services.AddScoped<ICurrentUserService, CurrentUserService>(); // Resolves the User context per request
        services.AddTransient<IIdentityService, IdentityService>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        // 7. Register AI Matchmaking Typed HTTP Client
        services.AddHttpClient<IAiMatchmakingClient, AiMatchmakingHttpClient>(client =>
        {
            var baseUrl = configuration["AiEngine:BaseUrl"]
                ?? throw new InvalidOperationException("AI Engine BaseUrl configuration 'AiEngine:BaseUrl' was not found.");

            var timeoutInSeconds = configuration.GetValue<int?>("AiEngine:TimeoutInSeconds") ?? 10;

            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
        });

        return services;
    }
}