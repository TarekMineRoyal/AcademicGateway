using AcademicGateway.Application.Features.ProjectTemplates.Queries.GetApprovedTemplates;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.ProjectTemplates.Queries.GetApprovedTemplates;

/// <summary>
/// Endpoint for discovering approved project template blueprints.
/// </summary>
[ApiController]
[Tags("Project Templates")]
[Authorize] // Enforce an authenticated user session to check technical placement options
[Route("api/project-templates/approved")]
public class GetApprovedTemplatesController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Retrieves a collection of approved, publicly available project template blueprints, optionally filtered by a specific skill.
    /// </summary>
    /// <param name="skillId">The optional unique tracking identifier of a technical skill lookup constraint.</param>
    /// <returns>A 200 OK response containing the collection of matching verified placement templates.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetApproved([FromQuery] Guid? skillId)
    {
        var query = new GetApprovedTemplatesQuery { SkillId = skillId };
        var templates = await mediator.Send(query);

        return Ok(templates);
    }
}