using System;
using AcademicGateway.Domain.Common;

namespace AcademicGateway.Domain.Students.Events;

/// <summary>
/// Domain event raised whenever an existing Student profile or its relational mappings are updated.
/// </summary>
public record StudentUpdatedEvent(Guid StudentId) : IDomainEvent;