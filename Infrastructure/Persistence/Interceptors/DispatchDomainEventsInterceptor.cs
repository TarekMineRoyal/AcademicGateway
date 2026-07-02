using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Infrastructure.Persistence.Interceptors;

/// <summary>
/// A specialized Entity Framework Core SaveChangesInterceptor that intercepts the persistence pipeline.
/// Automatically extracts, clears, and dispatches queued internal domain event side-effects natively 
/// before state mutations are committed down to the relational database provider.
/// </summary>
public class DispatchDomainEventsInterceptor(IServiceProvider serviceProvider) : SaveChangesInterceptor
{
    /// <summary>
    /// Synchronous interception hook executed prior to saving modifications.
    /// Blocks the current thread to safely dispatch domain events before falling back to base execution.
    /// </summary>
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context != null)
        {
            // Fallback for unexpected synchronous execution routes.
            // Blocks asynchronously designed domain event handlers safely using GetAwaiter().GetResult().
            DispatchDomainEventsAsync(eventData.Context, default).GetAwaiter().GetResult();
        }

        return base.SavingChanges(eventData, result);
    }

    /// <summary>
    /// Asynchronous interception hook executed natively by the Entity Framework Core pipeline right before committing changes.
    /// Extracts and completely flushes outstanding domain events in a continuous cascading pipeline loop.
    /// </summary>
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context != null)
        {
            await DispatchDomainEventsAsync(eventData.Context, cancellationToken);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// Scans the database context change tracker for all aggregates tracking queued domain events, 
    /// flushes their internal collections to avoid cyclical loops, and dynamically routes them to registered handlers.
    /// </summary>
    private async Task DispatchDomainEventsAsync(DbContext context, CancellationToken cancellationToken)
    {
        // Utilizes a continuous evaluation loop to catch cascading events 
        // generated dynamically by downstream event handlers within the same execution unit-of-work.
        while (true)
        {
            var domainEntities = context.ChangeTracker
                .Entries<BaseEntity>()
                .Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any())
                .ToList();

            if (!domainEntities.Any())
            {
                break;
            }

            var domainEvents = domainEntities
                .SelectMany(x => x.Entity.DomainEvents)
                .ToList();

            // Clear tracking collections immediately prior to invocation to rule out infinity execution loops
            foreach (var domainEntity in domainEntities)
            {
                domainEntity.Entity.ClearDomainEvents();
            }

            foreach (var domainEvent in domainEvents)
            {
                // Formulate the distinct generic registration contract signature: IDomainEventHandler<TEvent>
                var handlerInterfaceType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());

                // Resolve all concrete registrations configured natively within the service container
                var handlers = serviceProvider.GetServices(handlerInterfaceType);

                foreach (var handler in handlers)
                {
                    if (handler == null) continue;

                    // Dynamically extract the HandleAsync method signature
                    var method = handlerInterfaceType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync));

                    if (method != null)
                    {
                        // Safely invoke the execution branch and forward required operational signals down
                        await (Task)method.Invoke(handler, new object[] { domainEvent, cancellationToken })!;
                    }
                }
            }
        }
    }
}