using AcademicGateway.Application.Features.ProviderApplications.Commands.ReviewProviderApplication;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicGateway.Api.Features.ProviderApplications.Review;

[Authorize(Roles = "Reviewer")]
[ApiController]
[Route("api/reviewers")]
public class ReviewApplicationController(ISender mediator) : ControllerBase
{
    [HttpPost("applications/{id:guid}/review")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ReviewApplication(Guid id, [FromBody] ReviewApplicationRequest request)
    {
        var command = new ReviewProviderApplicationCommand
        {
            ApplicationId = id,
            IsApproved = request.IsApproved,
            RejectionReason = request.RejectionReason
        };

        await mediator.Send(command);
        return NoContent();
    }
}

public record ReviewApplicationRequest(bool IsApproved, string? RejectionReason);