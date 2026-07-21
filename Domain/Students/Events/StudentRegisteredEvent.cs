using System;
using AcademicGateway.Domain.Common;

namespace AcademicGateway.Domain.Students.Events;

/// <summary>
/// Domain event raised whenever a new Student profile is registered.
/// </summary>
public record StudentRegisteredEvent(Guid StudentId) : IDomainEvent;