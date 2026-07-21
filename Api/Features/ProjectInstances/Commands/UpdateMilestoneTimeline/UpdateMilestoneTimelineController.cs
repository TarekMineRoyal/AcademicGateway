using AcademicGateway.Application.Features.ProjectInstances.Commands.UpdateMilestoneTimeline;
using AcademicGateway.Domain.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.ProjectInstances.Commands.UpdateMilestoneTimeline;

/// <summary>
/// API Request payload schema for adjusting the scheduling bounds of an active milestone.
/// </summary>
/// <param name="ScheduledStartDate">The student-proposed synchronized commencement timestamp for the selected milestone execution leg.</param>
/// <param name="ScheduledEndDate">The student-proposed completion deadline target timestamp for the selected milestone execution leg.</param>
public record UpdateMilestoneTimelineRequest(DateTime ScheduledStartDate, DateTime ScheduledEndDate);

/// <summary>
/// Single Action Controller endpoint allowing authenticated students to establish or readjust 
/// the target operational timeline and execution window of an active project instance milestone phase.
/// </summary>
[ApiController]
[Tags("Project Instances")]
[Authorize(Roles = $"{Roles.Student},{Roles.Professor}")]
[Route("api/project-instances/{projectInstanceId:guid}/milestones/{localMilestoneId:guid}/timeline")]
public class UpdateMilestoneTimelineController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Readjusts the execution timeline constraints and dates for an active milestone node.
    /// </summary>
    /// <param name="projectInstanceId">The tracking identifier key parsing out the targeted live project workspace root.</param>
    /// <param name="localMilestoneId">The unique tracking identifier of the target LocalMilestone within the instance workspace.</param>
    /// <param name="request">The incoming body parameters holding the student-proposed start and end dates.</param>
    /// <returns>A 204 No Content response confirming the calendar timeline rewrite was saved successfully.</returns>
    [HttpPatch]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTimeline(
        [FromRoute] Guid projectInstanceId,
        [FromRoute] Guid localMilestoneId,
        [FromBody] UpdateMilestoneTimelineRequest request)
    {
        // Hydrate the CQRS command object securely drawing from route paths and body properties
        var command = new UpdateMilestoneTimelineCommand
        {
            ProjectInstanceId = projectInstanceId,
            LocalMilestoneId = localMilestoneId,
            ScheduledStartDate = request.ScheduledStartDate,
            ScheduledEndDate = request.ScheduledEndDate
        };

        // Dispatch downstream into the application service pipeline boundary layer via MediatR bus
        await mediator.Send(command);

        // Standard REST pattern: Return 204 No Content for state mutations carrying no entity response body
        return NoContent();
    }
}