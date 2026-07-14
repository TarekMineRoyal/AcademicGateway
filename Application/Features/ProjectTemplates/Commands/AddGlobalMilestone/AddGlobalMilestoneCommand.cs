using MediatR;
using System;

namespace AcademicGateway.Application.Features.ProjectTemplates.Commands.AddGlobalMilestone;

/// <summary>
/// CQRS Command to append a new global milestone blueprint configuration onto an existing project template.
/// Executed exclusively within the template editing lifecycle by verified providers.
/// </summary>
public record AddGlobalMilestoneCommand : IRequest<Guid>
{
    /// <summary>
    /// Gets the unique tracking identifier of the target parent ProjectTemplate aggregate root.
    /// </summary>
    public Guid ProjectTemplateId { get; init; }

    /// <summary>
    /// Gets the operational title assigned to the milestone blueprint.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Gets the detailed text description mapping execution requirements and evaluation baselines.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets the estimated nominal effort duration calculated in hours.
    /// Used later for effort-based execution timeline generation.
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
}