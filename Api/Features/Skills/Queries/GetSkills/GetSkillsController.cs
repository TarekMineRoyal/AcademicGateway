using AcademicGateway.Application.Features.Skills.Queries.GetSkills;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.Skills.Queries.GetSkills;

/// <summary>
/// Endpoint for retrieving technical skills lookup metadata.
/// </summary>
[ApiController]
[Tags("Skills")]
[Route("api/skills")]
public class GetSkillsController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Retrieves the master dictionary of technical skills registered within the system framework.
    /// </summary>
    /// <returns>A 200 OK response containing the array of technical skill lookup items.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSkills()
    {
        var result = await mediator.Send(new GetSkillsQuery());
        return Ok(result);
    }
}