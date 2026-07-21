using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.Common.Constants;
using AcademicGateway.Application.Features.ProviderApplications.Commands.ReviewProviderApplication;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.ProviderApplications.Commands.ReviewProviderApplication;

/// <summary>
/// API Request payload schema for recording an administrative quality assessment decision on a provider application.
/// </summary>
public record ReviewApplicationRequest(bool IsApproved, string? RejectionReason);

/// <summary>
/// Single Action Controller endpoint allowing authorized institutional reviewers to process
/// corporate provider enrollment and compliance admission requests.
/// </summary>
[ApiController]
[Tags("Provider Applications")]
[Authorize(Roles = Roles.Reviewer)] // Enforce compliance security constraint boundaries
[Route("api/provider-applications/{applicationId:guid}/review")]
public class ReviewProviderApplicationController(
    ISender mediator,
    ICurrentUserService currentUserService) : ControllerBase
{
    /// <summary>
    /// Processes a pending corporate provider application, recording a terminal approval or rejection compliance state.
    /// </summary>
    /// <param name="applicationId">The unique tracking identifier of the target provider application captured from the route.</param>
    /// <param name="request">The evaluation payload containing approval status and necessary context notes.</param>
    /// <returns>A 204 No Content response confirming successful audit processing updates.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReviewApplication(
        [FromRoute] Guid applicationId,
        [FromBody] ReviewApplicationRequest request)
    {
        // Guard Invariant: Ensure the reviewer's authentication context tracks cleanly
        if (!currentUserService.UserId.HasValue)
        {
            return Unauthorized("Reviewer security context could not be resolved from the current token session.");
        }

        // Hydrate the CQRS command object securely mapping path, body, and token metadata parameters
        var command = new ReviewProviderApplicationCommand
        {
            ApplicationId = applicationId,
            ReviewerId = currentUserService.UserId.Value,
            IsApproved = request.IsApproved,
            RejectionReason = request.RejectionReason
        };

        await mediator.Send(command);

        // Return standard 204 No Content confirmation signaling that state machine modifications were successfully processed
        return NoContent();
    }
}