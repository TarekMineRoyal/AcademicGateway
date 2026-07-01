using MediatR;
using System;

namespace AcademicGateway.Application.Features.ProjectTemplates.Commands.ReviewTemplate;

/// <summary>
/// CQRS Command to process a formal quality assurance evaluation decision on a pending project blueprint.
/// Triggers terminal state-machine transitions within the ProjectTemplate aggregate root boundary.
/// </summary>
public record ReviewProjectTemplateCommand : IRequest
{
    /// <summary>
    /// Gets the unique identifier of the project template undergoing active review.
    /// </summary>
    public Guid TemplateId { get; init; }

    /// <summary>
    /// Gets the unique identifier of the reviewing institutional faculty member or auditor.
    /// Maps 1:1 to their underlying centralized user identity authentication record.
    /// </summary>
    public Guid ReviewerId { get; init; }

    /// <summary>
    /// Gets a value indicating whether the project template is authorized and approved for public student matching.
    /// </summary>
    public bool IsApproved { get; init; }

    /// <summary>
    /// Gets the administrative feedback or justification reasons explaining a negative evaluation decision.
    /// This parameter is strictly required when <see cref="IsApproved"/> evaluates to <c>false</c>.
    /// </summary>
    public string? RejectionReason { get; init; }
}