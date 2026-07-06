using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.ProjectInstances.Commands.FinalizeProject;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.ProjectInstances.Commands.FinalizeProject;

/// <summary>
/// Single Action Controller endpoint enabling authorized workspace participants (assigned faculty supervisors) to compute, compile, and permanently lock in the overall grade score
/// for a concluded project instance workspace.
/// </summary>
[ApiController]
[Tags("Project Instances")]
[Authorize(Roles = "Professor")]
[Route("api/project-instances/{projectInstanceId:guid}/finalize")]
public class FinalizeProjectController(
    ISender mediator,
    ICurrentUserService currentUserService) : ControllerBase
{
    /// <summary>
    /// Calculates, aggregates, and freezes the final performance scoring for an entire project instance workspace.
    /// </summary>
    /// <param name="projectInstanceId">The unique tracking identifier of the parent ProjectInstance aggregate root workspace to be certified.</param>
    /// <returns>A 204 No Content response confirming that the final evaluation matrix has been successfully compiled and locked.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FinalizeProject([FromRoute] Guid projectInstanceId)
    {
        // Guard Invariant: Ensure the calling participant security context is fully resolved
        if (!currentUserService.UserId.HasValue)
        {
            return Unauthorized("User security context could not be resolved.");
        }

        // Hydrate the CQRS command object securely drawing from route parameters and token user identities
        var command = new FinalizeProjectCommand
        {
            ProjectInstanceId = projectInstanceId,
            ExecutingUserId = currentUserService.UserId.Value
        };

        // Dispatch downstream into the application service pipeline boundary layer via MediatR bus
        await mediator.Send(command);

        // Standard REST pattern: Return 204 No Content for final state transitions returning no body payload
        return NoContent();
    }
}