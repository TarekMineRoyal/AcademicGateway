using System;
using AcademicGateway.Domain.Common;

namespace AcademicGateway.Domain.ProjectTemplates.Events;

/// <summary>
/// Represents the immutable domain event raised when a project blueprint successfully 
/// passes faculty review boundaries or has reviewer alterations verified by its creator.
/// </summary>
/// <remarks>
/// This event marks a terminal successful lifecycle transition to <c>Approved</c>. It is used to run cross-aggregate 
/// side effects, such as publishing the blueprint to the student-matching grid or alerting the creating provider.
/// </remarks>
/// <param name="TemplateId">The unique identifier of the certified project template.</param>
/// <param name="ProviderId">The unique identifier of the corporate provider who owns the live template.</param>
public record ProjectTemplateApprovedEvent(Guid TemplateId, Guid ProviderId) : IDomainEvent;