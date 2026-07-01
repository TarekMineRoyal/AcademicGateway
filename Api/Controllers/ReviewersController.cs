using AcademicGateway.Application.Features.Reviewers.Commands.ReviewApplication;
using AcademicGateway.Application.Features.ProjectTemplates.Commands.ReviewTemplate;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Reviewer")] // Strict global guard: Only identities assigned the 'Reviewer' role can access this controller
public class ReviewersController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Evaluates and transitions a pending corporate provider verification application slot.
    /// </summary>
    [HttpPost("applications/{id:guid}/review")]
    public async Task<IActionResult> ReviewApplication(Guid id, [FromBody] ReviewApplicationRequest request)
    {
        // Extract the unique identity security identifier from the claims token context
        var reviewerIdentityId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(reviewerIdentityId))
        {
            return Unauthorized("Reviewer security context could not be resolved from token metadata.");
        }

        var command = new ReviewProviderApplicationCommand
        {
            ApplicationId = id,
            ReviewerIdentityUserId = reviewerIdentityId,
            IsApproved = request.IsApproved,
            RejectionReason = request.RejectionReason
        };

        await mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Evaluates and transitions an incoming drafted project curriculum template slot.
    /// </summary>
    [HttpPost("templates/{id:guid}/review")]
    public async Task<IActionResult> ReviewTemplate(Guid id, [FromBody] ReviewTemplateRequest request)
    {
        var reviewerIdentityId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(reviewerIdentityId))
        {
            return Unauthorized("Reviewer security context could not be resolved from token metadata.");
        }

        var command = new ReviewProjectTemplateCommand
        {
            TemplateId = id,
            ReviewerIdentityUserId = reviewerIdentityId,
            IsApproved = request.IsApproved,
            RejectionReason = request.RejectionReason
        };

        await mediator.Send(command);
        return NoContent();
    }
}

// Local request transfer structures to maintain clean, decoupled endpoint contracts
public record ReviewApplicationRequest(bool IsApproved, string? RejectionReason);
public record ReviewTemplateRequest(bool IsApproved, string? RejectionReason);