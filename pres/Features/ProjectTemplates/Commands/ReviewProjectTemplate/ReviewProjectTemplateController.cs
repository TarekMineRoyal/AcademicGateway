using AcademicGateway.Application.Features.ProjectTemplates.Commands.ReviewProjectTemplate;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.ProjectTemplates.Commands.ReviewProjectTemplate;

/// <summary>
/// API Request payload schema for recording an administrative review assessment on a project blueprint template.
/// </summary>
public record ReviewTemplateRequest(bool IsApproved, string? RejectionReason);

/// <summary>
/// Single Action Controller endpoint allowing authorized academic reviewers to approve 
/// or reject a proposed project template blueprint draft.
/// </summary>
[ApiController]
[Tags("Project Templates")]
[Authorize(Roles = "Reviewer")] // Enforce that only users holding the Reviewer security role can access this endpoint
[Route("api/project-templates/{templateId:guid}/review")]
public class ReviewProjectTemplateController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Evaluates a submitted project template blueprint, recording an official approval or rejection decision status.
    /// </summary>
    /// <param name="templateId">The unique tracking identifier of the target project template blueprint captured from the route.</param>
    /// <param name="request">The decision payload containing approval boolean status and optional justification notes.</param>
    /// <returns>A 204 No Content response confirming successful persistence of the review state transition.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReviewTemplate(
        [FromRoute] Guid templateId,
        [FromBody] ReviewTemplateRequest request)
    {
        // Hydrate the CQRS application command object binding the route token and body elements cleanly
        var command = new ReviewProjectTemplateCommand
        {
            TemplateId = templateId,
            IsApproved = request.IsApproved,
            RejectionReason = request.RejectionReason
        };

        await mediator.Send(command);

        // Return standard 204 No Content confirming the template aggregate state modification was finalized
        return NoContent();
    }
}