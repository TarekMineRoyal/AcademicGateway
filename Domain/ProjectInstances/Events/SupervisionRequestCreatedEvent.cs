using System;
using AcademicGateway.Domain.Common;

namespace AcademicGateway.Domain.ProjectInstances.Events;

/// <summary>
/// Raised whenever a student submits a new matchmaking invitation (either at project startup or mid-lifecycle) 
/// to an academic supervisor.
/// </summary>
public record SupervisionRequestCreatedEvent(
    Guid SupervisionRequestId,
    Guid ProjectInstanceId,
    Guid ProfessorId,
    string PitchText) : IDomainEvent;