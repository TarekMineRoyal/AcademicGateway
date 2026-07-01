using AcademicGateway.Application.Features.Professors.Queries.GetProfessorProfile;
using AcademicGateway.Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicGateway.Api.Features.Professors.GetProfile;

/// <summary>
/// Endpoint for retrieving the authenticated professor profile.
/// </summary>
[Authorize(Roles = "Professor")]
[ApiController]
[Route("api/professors")]
public class GetProfessorProfileController(ISender mediator, ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet("profile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProfessorProfile()
    {
        if (!currentUserService.UserId.HasValue) return Unauthorized();

        var result = await mediator.Send(new GetProfessorProfileQuery(currentUserService.UserId.Value));
        return Ok(result);
    }
}