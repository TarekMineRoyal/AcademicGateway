using AcademicGateway.Application.Features.SupervisionRequests.Commands.ReviewSupervisionRequest;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.SupervisionRequests.Review;

/// <summary>
/// API Request payload schema for an academic professor to submit an approval or rejection review decision.
/// </summary>
public record ReviewSupervisionRequestRequest(bool Accept, string? RejectionReason);

/// <summary>
/// Single Action Controller endpoint allowing the targeted academic professor to accept or decline 
/// an outstanding matchmaking supervision invitation request linked to a specific project workspace.
/// </summary>
[Authorize(Roles = "Professor")] // Protect the academic evaluation channel with explicit role security boundaries
[ApiController]
[Route("api/project-instances/{projectInstanceId:guid}/supervision-requests/{requestId:guid}/review")]
public class ReviewSupervisionRequestController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Processes an academic evaluation decision on an outstanding supervision request log entry.
    /// </summary>
    /// <param name="projectInstanceId">The unique tracking identifier of the parent project workspace captured from the route segment.</param>
    /// <param name="requestId">The unique tracking identifier of the specific supervision invitation being evaluated from the route segment.</param>
    /// <param name="request">The decision choices payload containing the acceptance flag and optional rejection text comments.</param>
    /// <param name="cancellationToken">The system thread execution cancellation monitor hook.</param>
    /// <returns>A 24 No Content response confirming that the aggregate state machine has been synchronized.</returns>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Review(
        [FromRoute] Guid projectInstanceId,
        [FromRoute] Guid requestId,
        [FromBody] ReviewSupervisionRequestRequest request,
        CancellationToken cancellationToken)
    {
        // Hydrate the CQRS command object, decomposing multiple route segments and body parameters cleanly
        var command = new ReviewSupervisionRequestCommand
        {
            ProjectInstanceId = projectInstanceId,
            SupervisionRequestId = requestId,
            Accept = request.Accept,
            RejectionReason = request.RejectionReason
        };

        await mediator.Send(command, cancellationToken);

        // Return a standard RESTful confirmation indicating that server-side state modification completed successfully
        return NoContent();
    }
}