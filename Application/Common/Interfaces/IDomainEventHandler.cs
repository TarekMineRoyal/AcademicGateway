using AcademicGateway.Domain.Common;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Common.Interfaces;

/// <summary>
/// Defines a continuous, type-safe processing contract for executing asynchronous application side effects 
/// triggered by a specific, pure <see cref="IDomainEvent"/>.
/// </summary>
/// <typeparam name="TEvent">The target concrete implementation of <see cref="IDomainEvent"/> to consume.</typeparam>
/// <remarks>
/// This interface handles in-process decoupling. Concrete event handlers implementing this contract 
/// reside in the Application layer, keeping side-effect orchestration logic cleanly separated from 
/// the main command flow execution paths.
/// </remarks>
public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    /// <summary>
    /// Core execution hook invoked automatically when the corresponding domain event is dispatched.
    /// </summary>
    /// <param name="domainEvent">The contextual immutable event payload data instance.</param>
    /// <param name="cancellationToken">A token to observe and propagate cancellation requests across concurrent async operations.</param>
    /// <returns>A structured asynchronous tracking task representing the execution operation.</returns>
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken);
}