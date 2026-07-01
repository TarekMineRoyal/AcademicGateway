using AcademicGateway.Application.Features.Students.Queries.GetStudentProfile;
using AcademicGateway.Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicGateway.Api.Features.Students.GetProfile;

/// <summary>
/// Endpoint for retrieving the authenticated student profile.
/// </summary>
[Authorize] // Only authenticated students should see their own profile
[ApiController]
[Route("api/students")]
public class GetStudentProfileController(ISender mediator, ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet("profile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStudentProfile()
    {
        if (!currentUserService.UserId.HasValue) return Unauthorized();

        var result = await mediator.Send(new GetStudentProfileQuery(currentUserService.UserId.Value));
        return Ok(result);
    }
}