using System;
using AcademicGateway.Domain.Common;

namespace AcademicGateway.Domain.Professors.Events;

/// <summary>
/// Domain event raised whenever an existing Professor profile is updated.
/// </summary>
public record ProfessorUpdatedEvent(Guid ProfessorId) : IDomainEvent;