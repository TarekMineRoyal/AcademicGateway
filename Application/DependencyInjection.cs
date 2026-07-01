using AcademicGateway.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace AcademicGateway.Application;

/// <summary>
/// Provides centralized dependency injection extension hooks for the Application layer assembly.
/// Automatically handles the discovery and lifecycle mapping of core application behaviors.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Scans the application assembly and automatically registers all domain event handlers with the DI container.
    /// </summary>
    /// <param name="services">The centralized service collection container descriptor from the application host builder.</param>
    /// <returns>The modified <see cref="IServiceCollection"/> container to facilitate fluid configuration chaining.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // 1. Locate all non-abstract, concrete classes implementing our custom IDomainEventHandler<> contract
        var handlerRegistrations = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface &&
                        t.GetInterfaces().Any(i => i.IsGenericType &&
                                                   i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>)));

        // 2. Iterate through discovered implementations and register them to their closed generic definitions
        foreach (var concreteType in handlerRegistrations)
        {
            // Find the specific closed generic interface types implemented by the concrete class
            var matchingInterfaces = concreteType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>));

            foreach (var interfaceType in matchingInterfaces)
            {
                // Register each interface to map directly to its concrete handler implementation under Scoped lifecycles
                // This ensures handlers share the exact same DbContext transaction instance tracking the orchestrating command
                services.AddScoped(interfaceType, concreteType);
            }
        }

        return services;
    }
}