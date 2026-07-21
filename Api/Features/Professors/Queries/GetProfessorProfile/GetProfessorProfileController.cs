using AcademicGateway.Application.Features.Professors.Queries.GetProfessorProfile;
using AcademicGateway.Domain.Common.Constants;
using AcademicGateway.Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AcademicGateway.Api.Features.Professors.Queries.GetProfessorProfile;

/// <summary>
/// Endpoint for retrieving the authenticated professor profile.
/// </summary>
[Authorize(Roles = Roles.Professor)]
[ApiController]
[Tags("Professors")]
[Route("api/professors/profile")]
public class GetProfessorProfileController(ISender mediator, ICurrentUserService currentUserService) : ControllerBase
{
    /// <summary>
    /// Retrieves the profile details of the currently authenticated professor.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProfessorProfile()
    {
        if (!currentUserService.UserId.HasValue) return Unauthorized();

        var result = await mediator.Send(new GetProfessorProfileQuery(currentUserService.UserId.Value));
        return Ok(result);
    }
}