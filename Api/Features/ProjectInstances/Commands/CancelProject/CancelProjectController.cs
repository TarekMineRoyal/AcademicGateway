using AcademicGateway.Application.Features.ProjectInstances.Commands.CancelProject;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.ProjectInstances.Commands.CancelProject;

/// <summary>
/// API Request payload schema for prematurely aborting or abandoning a project workspace execution lifecycle runner.
/// </summary>
public record CancelProjectRequest(string? Reason);

/// <summary>
/// Single Action Controller endpoint allowing authenticated student workspace owners to prematurely 
/// abort, drop, or abandon an active running project instance workspace context boundary.
/// </summary>
[Authorize] // Enforce an authenticated user session so handlers can safely evaluate workspace ownership constraints
[ApiController]
[Tags("Project Instances")]
[Route("api/project-instances/{projectInstanceId:guid}/cancel")]
public class CancelProjectController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Prematurely aborts, drops, or abandons an active running project instance workspace.
    /// </summary>
    /// <param name="projectInstanceId">The unique tracking identifier of the target project workspace captured from the route segment.</param>
    /// <param name="request">The optional explanatory text details outlining the justification for aborting execution.</param>
    /// <param name="cancellationToken">The system thread execution cancellation monitor hook.</param>
    /// <returns>A 204 No Content response confirming that the aggregate state machine has been successfully aborted.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(
        [FromRoute] Guid projectInstanceId,
        [FromBody] CancelProjectRequest request,
        CancellationToken cancellationToken)
    {
        // Hydrate the CQRS command object mapping path variables and the body payload variables cleanly
        var command = new CancelProjectCommand
        {
            ProjectInstanceId = projectInstanceId,
            Reason = request.Reason
        };

        await mediator.Send(command, cancellationToken);

        // Return standard 204 No Content validation response indicating server state transition completed cleanly
        return NoContent();
    }
}