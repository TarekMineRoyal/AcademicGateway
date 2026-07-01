using AcademicGateway.Application.Features.Reviewers.Commands.ReviewApplication;
using AcademicGateway.Application.Features.ProjectTemplates.Commands.ReviewTemplate;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Api.Controllers.Reviewers;

/// <summary>
/// Manages review and verification workflows for corporate provider applications and project templates.
/// </summary>
[Authorize(Roles = "Reviewer")]
[ApiController]
[Route("api/[controller]")]
public class ReviewersController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Evaluates and transitions a pending corporate provider verification application.
    /// </summary>
    /// <param name="id">The unique identifier of the application to review.</param>
    /// <param name="request">The review decision details.</param>
    [HttpPost("applications/{id:guid}/review")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReviewApplication(Guid id, [FromBody] ReviewApplicationRequest request)
    {
        if (!TryGetReviewerId(out _)) // Verification of identity context
        {
            return Unauthorized("Reviewer security context could not be resolved from token metadata.");
        }

        var command = new ReviewProviderApplicationCommand
        {
            ApplicationId = id,
            IsApproved = request.IsApproved,
            RejectionReason = request.RejectionReason
        };

        await mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Evaluates and transitions an incoming drafted project curriculum template.
    /// </summary>
    /// <param name="id">The unique identifier of the template to review.</param>
    /// <param name="request">The review decision details.</param>
    [HttpPost("templates/{id:guid}/review")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReviewTemplate(Guid id, [FromBody] ReviewTemplateRequest request)
    {
        if (!TryGetReviewerId(out _))
        {
            return Unauthorized("Reviewer security context could not be resolved from token metadata.");
        }

        var command = new ReviewProjectTemplateCommand
        {
            TemplateId = id,
            IsApproved = request.IsApproved,
            RejectionReason = request.RejectionReason
        };

        await mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Helper to validate the Reviewer's identity context.
    /// </summary>
    private bool TryGetReviewerId(out Guid reviewerId)
    {
        var identityString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(identityString, out reviewerId);
    }
}

public record ReviewApplicationRequest(bool IsApproved, string? RejectionReason);
public record ReviewTemplateRequest(bool IsApproved, string? RejectionReason);