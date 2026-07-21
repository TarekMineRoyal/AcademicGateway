using AcademicGateway.Application.Features.Students.Queries.GetStudentProfile;
using AcademicGateway.Domain.Common.Constants;
using AcademicGateway.Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.Students.Queries.GetStudentProfile;

/// <summary>
/// Endpoint for retrieving the authenticated student profile.
/// </summary>
[Authorize(Roles = Roles.Student)]
[ApiController]
[Tags("Students")]
[Route("api/students/profile")]
public class GetStudentProfileController(ISender mediator, ICurrentUserService currentUserService) : ControllerBase
{
    /// <summary>
    /// Retrieves the comprehensive academic profile, mapped major programs, and technical skill matrices for the authenticated student.
    /// </summary>
    /// <returns>A 200 OK response carrying the student's aggregate profile data transfer object payload.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStudentProfile()
    {
        if (!currentUserService.UserId.HasValue) return Unauthorized();

        var result = await mediator.Send(new GetStudentProfileQuery(currentUserService.UserId.Value));
        return Ok(result);
    }
}