using System;
using AcademicGateway.Domain.Common;

namespace AcademicGateway.Domain.ProjectInstances.Events;

/// <summary>
/// Raised immediately when a student initializes a new live workspace snapshot from a project template blueprint.
/// </summary>
public record ProjectInstanceStartedEvent(
    Guid ProjectInstanceId,
    Guid TemplateId,
    Guid StudentId,
    Guid? InitialRequestedProfessorId) : IDomainEvent;