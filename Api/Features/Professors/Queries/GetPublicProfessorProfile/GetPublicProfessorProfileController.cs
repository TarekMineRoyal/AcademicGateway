using System;
using System.Threading.Tasks;
using AcademicGateway.Application.Features.Professors.Queries.GetPublicProfessorProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AcademicGateway.Api.Features.Professors.Queries.GetPublicProfessorProfile;

/// <summary>
/// Controller endpoint for retrieving an individual professor's public profile by their unique identifier.
/// </summary>
[Authorize]
[ApiController]
[Tags("Professors")]
[Route("api/v1/professors")]
public class GetPublicProfessorProfileController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Fetches the public profile details of a professor by ID.
    /// </summary>
    /// <param name="id">The unique identifier of the target professor.</param>
    /// <returns>The public professor profile details if found; otherwise, 404 Not Found.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetPublicProfessorProfileQueryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPublicProfessorProfile(Guid id)
    {
        var result = await mediator.Send(new GetPublicProfessorProfileQuery(id));
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }
}