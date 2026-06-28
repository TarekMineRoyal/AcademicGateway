using AcademicGateway.Application.Features.Lookups.Queries.GetMajors;
using AcademicGateway.Application.Features.Lookups.Queries.GetSkills;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LookupsController(ISender mediator) : ControllerBase
{
    [HttpGet("majors")]
    public async Task<IActionResult> GetMajors()
    {
        var result = await mediator.Send(new GetMajorsWithSpecialtiesQuery());
        return Ok(result);
    }

    [HttpGet("skills")]
    public async Task<IActionResult> GetSkills()
    {
        var result = await mediator.Send(new GetSkillsQuery());
        return Ok(result);
    }
}