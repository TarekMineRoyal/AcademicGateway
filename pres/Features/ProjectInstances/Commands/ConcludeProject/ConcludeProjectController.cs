using AcademicGateway.Application.Features.ProjectInstances.Commands.ConcludeProject;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.ProjectInstances.Commands.ConcludeProject;

/// <summary>
/// Single Action Controller endpoint allowing authenticated student workspace owners to cleanly 
/// close down and successfully wrap up a finished project instance.
/// </summary>
[Authorize(Roles = "Student")]
[ApiController]
[Tags("Project Instances")]
[Route("api/project-instances/{projectInstanceId:guid}/conclude")]
public class ConcludeProjectController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Cleanly closes down and successfully wraps up a finished project instance.
    /// </summary>
    /// <param name="projectInstanceId">The unique tracking identifier of the target project instance workspace captured from the route segment.</param>
    /// <param name="cancellationToken">The system thread execution cancellation monitor hook.</param>
    /// <returns>A 204 No Content response confirming successful administrative workspace closure.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Conclude(
        [FromRoute] Guid projectInstanceId,
        CancellationToken cancellationToken)
    {
        // Hydrate the CQRS command object, binding the unique identifier from the route segment cleanly
        var command = new ConcludeProjectCommand
        {
            ProjectInstanceId = projectInstanceId
        };

        await mediator.Send(command, cancellationToken);

        // Return a standard 204 No Content response to confirm the final project state machine transition
        return NoContent();
    }
}