using System;
using AcademicGateway.Domain.Common;

namespace AcademicGateway.Domain.Students.Events;

/// <summary>
/// Domain event raised whenever a Student profile is removed from the system.
/// </summary>
public record StudentDeletedEvent(Guid StudentId) : IDomainEvent;