using MediatR;
using System;

namespace AcademicGateway.Application.Features.ProjectTemplates.Commands.UpdateGlobalMilestone;

/// <summary>
/// CQRS Command object carrying the request payload required to modify the specifications 
/// of an existing global milestone blueprint node within an under-construction project template.
/// </summary>
public class UpdateGlobalMilestoneCommand : IRequest
{
    /// <summary>
    /// Gets the primary tracking key identifying the parent ProjectTemplate aggregate root.
    /// </summary>
    public Guid ProjectTemplateId { get; init; }

    /// <summary>
    /// Gets the tracking identifier code of the specific milestone blueprint node to modify.
    /// </summary>
    public Guid MilestoneId { get; init; }

    /// <summary>
    /// Gets the newly updated descriptive headline title assigned to the milestone phase.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Gets the revised contextual parameters mapping work item goals and academic scope.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets the nominal estimation metrics mapping work effort constraints measured in hours.
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