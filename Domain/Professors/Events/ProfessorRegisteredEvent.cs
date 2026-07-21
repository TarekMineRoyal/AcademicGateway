using System;
using AcademicGateway.Domain.Common;

namespace AcademicGateway.Domain.Professors.Events;

/// <summary>
/// Domain event raised whenever a new Professor profile is registered.
/// </summary>
public record ProfessorRegisteredEvent(Guid ProfessorId) : IDomainEvent;