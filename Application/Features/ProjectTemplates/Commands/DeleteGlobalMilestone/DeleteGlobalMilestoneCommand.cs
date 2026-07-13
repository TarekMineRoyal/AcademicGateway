using MediatR;
using System;

namespace AcademicGateway.Application.Features.ProjectTemplates.Commands.DeleteGlobalMilestone;

/// <summary>
/// CQRS Command object carrying the request identifiers required to permanently remove 
/// a global milestone blueprint node from an under-construction project template graph.
/// </summary>
public class DeleteGlobalMilestoneCommand : IRequest
{
    /// <summary>
    /// Gets or sets the primary tracking key identifying the parent ProjectTemplate aggregate root.
    /// </summary>
    public Guid ProjectTemplateId { get; set; }

    /// <summary>
    /// Gets or sets the tracking identifier code of the specific milestone blueprint node to delete.
    /// </summary>
    public Guid MilestoneId { get; set; }
}