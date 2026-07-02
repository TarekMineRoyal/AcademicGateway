using MediatR;
using System;
using System.Collections.Generic;

namespace AcademicGateway.Application.Features.ProjectTemplates.Commands.CreateProjectTemplate;

/// <summary>
/// CQRS Command to initialize a new reusable project blueprint draft.
/// Instantiates an underlying ProjectTemplate aggregate root and registers its baseline required skills.
/// </summary>
public record CreateProjectTemplateCommand : IRequest<Guid>
{
    /// <summary>
    /// Gets the unique entity identifier of the corporate industry provider creating the template blueprint.
    /// This parameter is typically resolved from the authenticated corporate user session token contexts.
    /// </summary>
    public Guid ProviderId { get; init; }

    /// <summary>
    /// Gets the headline promotional title assigned to the project template blueprint.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Gets the comprehensive text description outlining requirements, execution scopes, and deliverables.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets the read-only sequence of global unique skill lookup identifiers requested as mandatory for student applications.
    /// </summary>
    public IReadOnlyCollection<Guid> SkillIds { get; init; } = Array.Empty<Guid>();
}