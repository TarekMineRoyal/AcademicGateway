using AcademicGateway.Domain.Common.Enums;
using MediatR;
using System;

namespace AcademicGateway.Application.Features.ProjectTemplates.Commands.AddGlobalTask;

/// <summary>
/// CQRS Command to append a brand new nested global task blueprint configuration onto an existing global milestone container.
/// Executed within the template editing lifecycle by verified providers.
/// </summary>
public record AddGlobalTaskCommand : IRequest<Guid>
{
    /// <summary>
    /// Gets the unique tracking identifier of the target parent ProjectTemplate aggregate root.
    /// </summary>
    public Guid ProjectTemplateId { get; init; }

    /// <summary>
    /// Gets the unique tracking identifier of the target global milestone container within the project template boundary.
    /// </summary>
    public Guid GlobalMilestoneId { get; init; }

    /// <summary>
    /// Gets the operational title assigned to the nested task blueprint.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Gets the detailed text description outlining the requirements and expectations for the task.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets the operational weight percentage assigned to this localized task relative to its parent milestone container.
    /// </summary>
    public decimal Weight { get; init; }

    /// <summary>
    /// Gets the expected submission format constraint mandated for student work execution under this task blueprint.
    /// </summary>
    public DeliverableType RequiredDeliverableType { get; init; }
}