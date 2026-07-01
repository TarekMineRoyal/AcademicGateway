using System.Linq;
using System.Reflection;
using AcademicGateway.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AcademicGateway.Application;

/// <summary>
/// Provides centralized dependency injection extension hooks for the Application layer assembly.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Scans the application assembly and automatically registers all domain event handlers with the DI container.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Locate all non-abstract, concrete classes implementing our custom IDomainEventHandler<> contract
        var handlerRegistrations = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface &&
                        t.GetInterfaces().Any(i => i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>)));

        foreach (var concreteType in handlerRegistrations)
        {
            // Find the specific closed generic interface types implemented by the concrete class
            var matchingInterfaces = concreteType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>));

            foreach (var interfaceType in matchingInterfaces)
            {
                // Register each interface to map directly to its concrete handler implementation
                services.AddScoped(interfaceType, concreteType);
            }
        }

        return services;
    }
}