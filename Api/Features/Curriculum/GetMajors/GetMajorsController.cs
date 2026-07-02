using AcademicGateway.Application.Features.Curriculum.Queries.GetMajorsWithSpecialties;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AcademicGateway.Api.Features.Curriculum.GetMajors;

/// <summary>
/// Endpoint for retrieving academic majors.
/// </summary>
[ApiController]
[Route("api/lookups")]
public class GetMajorsController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Retrieves the complete dictionary of academic majors and their associated specialty tracks.
    /// </summary>
    [HttpGet("majors")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMajors()
    {
        var result = await mediator.Send(new GetMajorsWithSpecialtiesQuery());
        return Ok(result);
    }
}