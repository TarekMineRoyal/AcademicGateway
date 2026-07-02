using System;
using AcademicGateway.Domain.Common;

namespace AcademicGateway.Domain.ProjectTemplates.Events;

/// <summary>
/// Represents the immutable domain event raised when a corporate provider declines a reviewer's 
/// proposed alterations, reverting the blueprint layout back to a manual draft status.
/// </summary>
public record ProjectTemplateReviewerChangesRejectedEvent(Guid TemplateId, Guid ProviderId) : IDomainEvent;