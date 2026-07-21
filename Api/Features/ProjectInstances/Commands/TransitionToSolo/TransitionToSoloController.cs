using AcademicGateway.Application.Features.ProjectInstances.Commands.TransitionToSolo;
using AcademicGateway.Domain.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.ProjectInstances.Commands.TransitionToSolo;

/// <summary>
/// Single Action Controller endpoint allowing authenticated student workspace owners to break out of 
/// the academic matchmaking loop and transition their project workspace into an un-supervised solo track.
/// </summary>
[Authorize(Roles = Roles.Student)]
[ApiController]
[Tags("Project Instances")]
[Route("api/project-instances/{projectInstanceId:guid}/transition-to-solo")]
public class TransitionToSoloController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Safely breaks out of the academic matchmaking loop to execute the project instance as a solo track.
    /// </summary>
    /// <param name="projectInstanceId">The unique tracking identifier of the target project instance workspace captured from the route segment.</param>
    /// <param name="cancellationToken">The system thread execution cancellation monitor hook.</param>
    /// <returns>A 204 No Content response confirming successful state transition tracking updates.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TransitionToSolo(
        [FromRoute] Guid projectInstanceId,
        CancellationToken cancellationToken)
    {
        // Build the CQRS command, binding the unique project instance identifier from the route
        var command = new TransitionToSoloCommand
        {
            ProjectInstanceId = projectInstanceId
        };

        await mediator.Send(command, cancellationToken);

        // Return a standard 204 No Content response to signal successful command processing
        return NoContent();
    }
}