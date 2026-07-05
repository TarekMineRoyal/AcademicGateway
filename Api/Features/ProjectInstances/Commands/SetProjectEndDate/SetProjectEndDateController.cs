using AcademicGateway.Application.Features.ProjectInstances.Commands.SetProjectEndDate;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.ProjectInstances.Commands.SetProjectEndDate;

/// <summary>
/// API Request payload schema for adjusting or extending a project workspace completion milestone deadline.
/// </summary>
public record SetProjectEndDateRequest(DateTime NewEndDate);

/// <summary>
/// Single Action Controller endpoint allowing authorized academic supervisors to adjust or extend 
/// the official operational completion deadline calendar target for a running project workspace runner.
/// </summary>
[Authorize(Roles = "Professor")] // Academic supervisors govern administrative calendar adjustments
[ApiController]
[Tags("Project Instances")]
[Route("api/project-instances/{projectInstanceId:guid}/end-date")]
public class SetProjectEndDateController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Adjusts or extends the official operational completion deadline for a running project instance workspace.
    /// </summary>
    /// <param name="projectInstanceId">The unique tracking identifier of the target project workspace captured from the route segment.</param>
    /// <param name="request">The payload containing the newly proposed calendar date time target threshold.</param>
    /// <param name="cancellationToken">The system thread execution cancellation monitor hook.</param>
    /// <returns>A 204 No Content response confirming successful deadline revision tracking state synchronization.</returns>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetEndDate(
        [FromRoute] Guid projectInstanceId,
        [FromBody] SetProjectEndDateRequest request,
        CancellationToken cancellationToken)
    {
        // Hydrate the CQRS command cleanly binding the path identity key and body target threshold date
        var command = new SetProjectEndDateCommand
        {
            ProjectInstanceId = projectInstanceId,
            NewEndDate = request.NewEndDate
        };

        await mediator.Send(command, cancellationToken);

        // Return standard 204 No Content validation response indicating server state update completed smoothly
        return NoContent();
    }
}