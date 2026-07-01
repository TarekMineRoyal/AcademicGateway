using AcademicGateway.Application.Features.ProjectTemplates.Commands.ReviewTemplate;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicGateway.Api.Features.ProjectTemplates.ReviewTemplate;

[Authorize(Roles = "Reviewer")]
[ApiController]
[Route("api/reviewers")] // Keep the route consistent
public class ReviewTemplateController(ISender mediator) : ControllerBase
{
    [HttpPost("templates/{id:guid}/review")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ReviewTemplate(Guid id, [FromBody] ReviewTemplateRequest request)
    {
        var command = new ReviewProjectTemplateCommand
        {
            TemplateId = id,
            IsApproved = request.IsApproved,
            RejectionReason = request.RejectionReason
        };

        await mediator.Send(command);
        return NoContent();
    }
}

public record ReviewTemplateRequest(bool IsApproved, string? RejectionReason);