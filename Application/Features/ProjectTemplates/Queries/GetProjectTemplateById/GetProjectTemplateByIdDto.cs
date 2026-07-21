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
    /// Gets the optional target academic major identifier restricting recommendation vector scans.
    /// </summary>
    public Guid? MajorId { get; init; }

    /// <summary>
    /// Gets the optional descriptive name of the target academic major.
    /// </summary>
    public string? MajorName { get; init; }

    /// <summary>
    /// Gets the optional target academic specialty identifier restricting recommendation vector scans.
    /// </summary>
    public Guid? SpecialtyId { get; init; }

    /// <summary>
    /// Gets the optional descriptive name of the target academic specialty.
    /// </summary>
    public string? SpecialtyName { get; init; }

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
public record TemplateSkillDto(Guid Id, string Name);

/// <summary>
/// Sub-DTO carrying structural node metadata for an abstract milestone stage configured on the template blueprint.
/// </summary>
public record TemplateGlobalMilestoneDto
{
    /// <summary>
    /// Gets the unique identifier for this global milestone blueprint node.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the operational title assigned to the milestone blueprint phase.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Gets the detailed text description mapping execution requirements and goals.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets the estimated nominal effort duration calculated in hours.
    /// </summary>
    public decimal ExpectedEffortInHours { get; init; }

    /// <summary>
    /// Gets the operational work breakdown structure (WBS) weight percentage relative to the total project effort.
    /// </summary>
    public decimal WbsWeight { get; init; }

    /// <summary>
    /// Gets the academic grading score weight contribution percentage relative to the total project score.
    /// </summary>
    public decimal GradingWeight { get; init; }

    /// <summary>
    /// Gets the hierarchical sub-collection of localized task requirements nested under this milestone blueprint container.
    /// </summary>
    public IReadOnlyCollection<GlobalTaskDto> Tasks { get; init; } = Array.Empty<GlobalTaskDto>();
}

/// <summary>
/// Sub-DTO tracking details of a specific localized task nested within a global milestone blueprint definition.
/// </summary>
public record GlobalTaskDto(
    Guid Id,
    string Title,
    string Description,
    decimal Weight,
    DeliverableType RequiredDeliverableType);

/// <summary>
/// Sub-DTO tracking directed scheduling relationship edges inside the project template's milestone graph network.
/// </summary>
public record TemplateMilestoneDependencyDto(
    Guid PredecessorId,
    Guid SuccessorId,
    DependencyType Type);