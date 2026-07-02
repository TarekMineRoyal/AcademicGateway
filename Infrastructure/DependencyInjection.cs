using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Infrastructure.Identity;
using AcademicGateway.Infrastructure.Persistence.Interceptors;
using AcademicGateway.Infrastructure.Services;
using Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

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

        return services;
    }
}