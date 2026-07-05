using MediatR;
using System;

namespace AcademicGateway.Application.Features.ProjectInstances.Commands.UpdateMilestoneTimeline;

/// <summary>
/// CQRS Command invoked by a student to establish or readjust the execution dates of a specific local milestone.
/// This acts as the entry vehicle for student-driven timeline planning under Rule 3 (Effort-Based Scheduling).
/// </summary>
public record UpdateMilestoneTimelineCommand : IRequest<Unit>
{
    /// <summary>
    /// Gets the unique tracking identifier of the parent ProjectInstance aggregate root workspace.
    /// </summary>
    public Guid ProjectInstanceId { get; init; }

    /// <summary>
    /// Gets the unique tracking identifier of the target LocalMilestone within the instance workspace.
    /// </summary>
    public Guid LocalMilestoneId { get; init; }

    /// <summary>
    /// Gets the student-proposed synchronized commencement timestamp for the selected milestone execution leg.
    /// </summary>
    public DateTime ScheduledStartDate { get; init; }

    /// <summary>
    /// Gets the student-proposed completion deadline target timestamp for the selected milestone execution leg.
    /// </summary>
    public DateTime ScheduledEndDate { get; init; }
}