using System;
using AcademicGateway.Domain.Common;

namespace AcademicGateway.Domain.Professors.Events;

/// <summary>
/// Domain event raised whenever a Professor profile is removed from the system.
/// </summary>
public record ProfessorDeletedEvent(Guid ProfessorId) : IDomainEvent;