using AcademicGateway.Application.Features.ProjectTemplates.Queries.GetApprovedTemplates;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicGateway.Api.Features.ProjectTemplates.GetApproved;

[ApiController]
[Route("api/templates")]
public class GetApprovedTemplatesController(ISender mediator) : ControllerBase
{
    [HttpGet("approved")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ApprovedTemplateDto>>> GetApproved([FromQuery] Guid? skillId)
    {
        var query = new GetApprovedTemplatesQuery { SkillId = skillId };
        var templates = await mediator.Send(query);
        return Ok(templates);
    }
}