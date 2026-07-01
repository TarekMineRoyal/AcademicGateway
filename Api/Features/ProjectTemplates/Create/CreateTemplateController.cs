using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.ProjectTemplates.Commands.CreateTemplate;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicGateway.Api.Features.ProjectTemplates.Create;

[ApiController]
[Route("api/templates")]
public class CreateTemplateController(
    ISender mediator,
    ICurrentUserService currentUserService) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Provider")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateTemplateRequest request)
    {
        if (!currentUserService.UserId.HasValue)
        {
            return Unauthorized("Provider corporate security context could not be resolved.");
        }

        var command = new CreateProjectTemplateCommand
        {
            ProviderId = currentUserService.UserId.Value,
            Title = request.Title,
            Description = request.Description,
            SkillIds = request.SkillIds
        };

        var templateId = await mediator.Send(command);

        return Ok(templateId);
    }
}

public record CreateTemplateRequest(string Title, string Description, List<Guid> SkillIds);