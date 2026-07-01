using AcademicGateway.Application.Features.Lookups.Queries.GetSkills;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AcademicGateway.Api.Features.Lookups.GetSkills;

/// <summary>
/// Endpoint for retrieving system technical skills.
/// </summary>
[ApiController]
[Route("api/lookups")]
public class GetSkillsController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Retrieves the master list of technical skills registered within the system.
    /// </summary>
    [HttpGet("skills")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSkills()
    {
        var result = await mediator.Send(new GetSkillsQuery());
        return Ok(result);
    }
}