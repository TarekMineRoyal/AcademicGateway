using AcademicGateway.Application.Features.Lookups.Queries.GetMajors;
using AcademicGateway.Application.Features.Lookups.Queries.GetSkills;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Api.Controllers.Lookups;

/// <summary>
/// Provides access to static, reference, or dictionary data used for system-wide configuration and selection lists.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class LookupsController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Retrieves the complete dictionary of academic majors and their associated specialty tracks.
    /// Used for populating cascading dropdowns or academic filters.
    /// </summary>
    [HttpGet("majors")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMajors()
    {
        var result = await mediator.Send(new GetMajorsWithSpecialtiesQuery());
        return Ok(result);
    }

    /// <summary>
    /// Retrieves the master list of technical skills registered within the system.
    /// Used for skill-based search or assignment tagging.
    /// </summary>
    [HttpGet("skills")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSkills()
    {
        var result = await mediator.Send(new GetSkillsQuery());
        return Ok(result);
    }
}