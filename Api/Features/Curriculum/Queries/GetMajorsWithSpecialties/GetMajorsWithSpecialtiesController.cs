using AcademicGateway.Application.Features.Curriculum.Queries.GetMajorsWithSpecialties;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AcademicGateway.Api.Features.Curriculum.Queries.GetMajorsWithSpecialties;

/// <summary>
/// Endpoint for retrieving academic majors and their specialties.
/// </summary>
[ApiController]
[AllowAnonymous]
[Tags("Curriculum")]
[Route("api/curriculum/majors")]
public class GetMajorsWithSpecialtiesController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Retrieves the complete dictionary of academic majors and their associated specialty tracks.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMajors()
    {
        var result = await mediator.Send(new GetMajorsWithSpecialtiesQuery());
        return Ok(result);
    }
}