using AcademicGateway.Application.Features.ProjectTemplates.Queries.GetProjectTemplateById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.ProjectTemplates.Queries.GetProjectTemplateById;

/// <summary>
/// Single Action Controller endpoint enabling authenticated application users to retrieve
/// the comprehensive structural matrix, milestone nodes, and dependency edges of a project template blueprint.
/// </summary>
[ApiController]
[Tags("Project Templates")]
[Authorize] // Available to all authenticated ecosystem participants needing to read blueprint structures
[Route("api/project-templates/{projectTemplateId:guid}")]
public class GetProjectTemplateByIdController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Fetches the deep operational graph configurations and milestone parameters for a requested project template.
    /// </summary>
    /// <param name="projectTemplateId">The primary tracking key identifying the targeted ProjectTemplate aggregate root.</param>
    /// <returns>A 200 OK status containing the granular structural snapshot payload, or a 404 Not Found if missing.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProjectTemplateDetailDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTemplateById([FromRoute] Guid projectTemplateId)
    {
        // Hydrate the CQRS query record parameter directly using the inbound route tracking identifier
        var query = new GetProjectTemplateByIdQuery { Id = projectTemplateId };

        // Dispatch via MediatR bus to pull untracked relational projections down from the database
        var templateDetails = await mediator.Send(query);

        // Guard Invariant: If the database projection returns null, bubble back a clean RESTful 404
        if (templateDetails == null)
        {
            return NotFound($"Project template blueprint with ID '{projectTemplateId}' could not be located.");
        }

        // Return the fully complete, mapped structural snapshot data transfer object
        return Ok(templateDetails);
    }
}