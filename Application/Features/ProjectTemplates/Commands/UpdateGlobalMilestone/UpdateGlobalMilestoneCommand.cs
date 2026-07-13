using AcademicGateway.Domain.Common.Enums;
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
    /// Gets or sets the primary tracking key identifying the parent ProjectTemplate aggregate root.
    /// </summary>
    public Guid ProjectTemplateId { get; set; }

    /// <summary>
    /// Gets or sets the tracking identifier code of the specific milestone blueprint node to modify.
    /// </summary>
    public Guid MilestoneId { get; set; }

    /// <summary>
    /// Gets or sets the newly updated descriptive headline title assigned to the milestone phase.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the revised contextual parameters mapping work item goals and academic scope.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the nominal estimation metrics mapping work effort constraints measured in hours.
    /// </summary>
    public decimal ExpectedEffortInHours { get; set; }

    /// <summary>
    /// Gets or sets the explicit deliverable tracking submission constraint rule token (e.g., File, Url).
    /// </summary>
    public DeliverableType RequiredDeliverableType { get; set; }
}