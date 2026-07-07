using MediatR;
using System;
using System.Collections.Generic;

namespace AcademicGateway.Application.Features.ProjectTemplates.Commands.ResubmitProjectTemplate;

/// <summary>
/// CQRS Command to modify and resubmit an existing project template blueprint currently in a non-terminal review status.
/// Updates the template specification details and technical skill requirements matrix before re-triggering the evaluation pipeline loop.
/// </summary>
public record ResubmitProjectTemplateCommand : IRequest<Guid>
{
    /// <summary>
    /// Gets the unique entity identifier of the target project template blueprint being updated and resubmitted.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the corrected or refined promotional title assigned to the project template blueprint.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Gets the updated comprehensive text description outlining requirements, execution scopes, and deliverables.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets the updated read-only sequence of global unique skill lookup identifiers requested as mandatory for student applications.
    /// </summary>
    public IReadOnlyCollection<Guid> SkillIds { get; init; } = Array.Empty<Guid>();
}