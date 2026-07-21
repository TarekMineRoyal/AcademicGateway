using AcademicGateway.Application.Features.Skills.Queries.GetUserSkills;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.Skills.Queries.GetUserSkills;

/// <summary>
/// Endpoint for retrieving user-specific technical skills portfolio allocations.
/// </summary>
[ApiController]
[Tags("Skills")]
[Authorize]
[Route("api/skills")]
public class GetUserSkillsController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Retrieves the technical capabilities, professional competencies, and skills associated explicitly with the requested user tracking reference.
    /// </summary>
    /// <param name="userId">The unique tracking primary key assigned to the targeted user identity profile.</param>
    /// <returns>A 200 OK response containing the collection of competency areas assigned onto the user's profile account data matrix.</returns>
    [HttpGet("user/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserSkills([FromRoute] Guid userId)
    {
        var result = await mediator.Send(new GetUserSkillsQuery(userId));
        return Ok(result);
    }
}