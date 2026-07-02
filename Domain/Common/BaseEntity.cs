using System;
using System.Collections.Generic;

namespace AcademicGateway.Domain.Common;

/// <summary>
/// Serves as the fundamental base class for all entities or aggregate roots capable of 
/// generating, tracking, and dispatching internal domain events natively.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// The backing tracking collection used to queue raised domain events before they are persisted.
    /// </summary>
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Gets the read-only tracking snapshot of domain events emitted during the current execution lifetime.
    /// Exposing this as an <see cref="IReadOnlyCollection{T}"/> prevents consumer layers from manipulating the queue directly.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Safely appends an internal state event message to this entity's dispatch queue.
    /// Accessible exclusively by inheriting aggregate structures during public method executions.
    /// </summary>
    /// <param name="domainEvent">The domain event payload instance to queue.</param>
    /// <exception cref="ArgumentNullException">Thrown if the provided domain event reference is null.</exception>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
        {
            throw new ArgumentNullException(nameof(domainEvent), "Domain event payload cannot be null.");
        }

        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Flushes and wipes the active event dispatch queue. 
    /// Typically executed by the persistence infrastructure immediately following a successful database commit.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}