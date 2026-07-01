namespace Domain.Common;

/// <summary>
/// Defines a pure, dependency-free marker interface for all domain events within the gateway.
/// Domain events represent exceptional, stateful occurrences or side effects within the aggregate 
/// boundaries that other sub-domains or bounded contexts care about.
/// </summary>
public interface IDomainEvent
{
}