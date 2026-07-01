using System;
using Domain.Common;

namespace Domain.ProjectTemplates.Events;

/// <summary>
/// Represents the immutable domain event raised when an academic reviewer optimizes a template's 
/// textual specifications and routes it to a confirmation hold for the provider's explicit sign-off.
/// </summary>
public record ProjectTemplateReviewerChangesProposedEvent(Guid TemplateId, Guid ProviderId) : IDomainEvent;