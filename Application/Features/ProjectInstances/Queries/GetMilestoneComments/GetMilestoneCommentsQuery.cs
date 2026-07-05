using MediatR;
using System;
using System.Collections.Generic;

namespace AcademicGateway.Application.Features.ProjectInstances.Queries.GetMilestoneComments;

/// <summary>
/// CQRS Query to retrieve the chronological collection feed of collaboration comments
/// posted within a specific milestone lane inside a live project workspace.
/// </summary>
public record GetMilestoneCommentsQuery : IRequest<List<MilestoneCommentDto>>
{
    /// <summary>
    /// Gets the unique lookup tracking identifier of the parent ProjectInstance aggregate workspace channel root.
    /// </summary>
    public Guid ProjectInstanceId { get; init; }

    /// <summary>
    /// Gets the unique tracking identifier of the localized checkpoint node whose chat feed is being fetched.
    /// </summary>
    public Guid LocalMilestoneId { get; init; }
}