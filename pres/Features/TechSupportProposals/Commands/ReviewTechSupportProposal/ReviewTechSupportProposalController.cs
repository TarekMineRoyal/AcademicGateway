using AcademicGateway.Application.Features.TechSupportProposals.Commands.ReviewTechSupportProposal;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.TechSupportProposals.Commands.ReviewTechSupportProposal;

/// <summary>
/// API Request payload schema for a student workspace owner to submit a corporate mentorship review decision.
/// </summary>
public record ReviewTechSupportProposalRequest(bool Accept, string? RejectionReason);

/// <summary>
/// Single Action Controller endpoint allowing the authenticated student owner of a project instance workspace 
/// to accept or decline a corporate technical support mentor assignment request.
/// </summary>
[Authorize(Roles = "Student")]
[ApiController]
[Tags("Tech Support Proposals")]
[Route("api/project-instances/{projectInstanceId:guid}/tech-support-proposals/{techSupportProposalId:guid}/review")]
public class ReviewTechSupportProposalController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Processes an administrative evaluation decision on an outstanding corporate tech support mentorship proposal.
    /// </summary>
    /// <param name="projectInstanceId">The unique tracking identifier of the parent project workspace captured from the route segment.</param>
    /// <param name="techSupportProposalId">The unique tracking identifier of the corporate tech support proposal being evaluated from the route segment.</param>
    /// <param name="request">The decision variables containing the acceptance flag and optional rejection text context.</param>
    /// <param name="cancellationToken">The system thread execution cancellation monitor hook.</param>
    /// <returns>A 204 No Content response confirming that the aggregate state machine has been successfully updated.</returns>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Review(
        [FromRoute] Guid projectInstanceId,
        [FromRoute] Guid techSupportProposalId,
        [FromBody] ReviewTechSupportProposalRequest request,
        CancellationToken cancellationToken)
    {
        // Construct the CQRS command payload, mapping route bounds and request body parameters cleanly
        var command = new ReviewTechSupportProposalCommand
        {
            ProjectInstanceId = projectInstanceId,
            TechSupportProposalId = techSupportProposalId,
            Accept = request.Accept,
            RejectionReason = request.RejectionReason
        };

        await mediator.Send(command, cancellationToken);

        // Return a standard 204 No Content response to confirm server state synchronization completed successfully
        return NoContent();
    }
}