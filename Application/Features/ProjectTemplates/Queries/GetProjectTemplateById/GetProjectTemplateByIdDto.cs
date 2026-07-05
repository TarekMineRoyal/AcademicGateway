using AcademicGateway.Domain.Common.Enums;
using AcademicGateway.Domain.ProjectTemplates.Enums;
using System;
using System.Collections.Generic;

namespace AcademicGateway.Application.Features.ProjectTemplates.Queries.GetProjectTemplateById;

/// <summary>
/// Data transfer object carrying complete structural configurations, node trees, 
/// and execution dependencies for a specific project template blueprint aggregate.
/// </summary>
public record ProjectTemplateDetailDto
{
    /// <summary>
    /// Gets the unique tracking identifier of the ProjectTemplate aggregate root.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the operational title assigned to the template blueprint.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Gets the detailed text description mapping execution scopes and requirements.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets the workflow review lifecycle phase state of the blueprint draft.
    /// </summary>
    public ProjectTemplateStatus Status { get; init; }

    /// <summary>
    /// Gets the unique user tracking identifier of the corporate provider organization that owns this template.
    /// </summary>
    public Guid ProviderId { get; init; }

    /// <summary>
    /// Gets the collection of prerequisite technical skill footprints linked to this project template configuration.
    /// </summary>
    public List<TemplateSkillDto> RequiredSkills { get; init; } = [];

    /// <summary>
    /// Gets the structural checkpoint timeline components defining the phase rules for this template.
    /// </summary>
    public List<TemplateGlobalMilestoneDto> Milestones { get; init; } = [];

    /// <summary>
    /// Gets the directed execution constraint edges mapping sequence workflows between milestone nodes.
    /// </summary>
    public List<TemplateMilestoneDependencyDto> Dependencies { get; init; } = [];
}

/// <summary>
/// Sub-DTO mapping out an explicitly associated technical capability requirement.
/// </summary>
public record TemplateSkillDto(Guid SkillId, string Name);

/// <summary>
/// Sub-DTO carrying structural node metadata for an abstract milestone stage configured on the template blueprint.
/// </summary>
public record TemplateGlobalMilestoneDto(
    Guid Id,
    string Title,
    string Description,
    decimal ExpectedEffortInHours,
    DeliverableType RequiredDeliverableType);

/// <summary>
/// Sub-DTO tracking directed scheduling relationship edges inside the project template's milestone graph network.
/// </summary>
public record TemplateMilestoneDependencyDto(
    Guid PredecessorId,
    Guid SuccessorId,
    DependencyType Type);