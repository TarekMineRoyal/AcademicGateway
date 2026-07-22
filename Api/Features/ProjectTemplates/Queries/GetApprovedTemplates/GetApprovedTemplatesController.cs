using AcademicGateway.Application.Common.Models;
using AcademicGateway.Application.Features.ProjectTemplates.Queries.GetApprovedTemplates;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
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
    /// Retrieves a paginated collection of approved, publicly available project template blueprints, optionally filtered by a specific skill.
    /// </summary>
    /// <param name="query">The query parameters including pagination parameters and optional skill lookup filter.</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be aborted.</param>
    /// <returns>A 200 OK response containing the paginated collection of matching verified placement templates.</returns>
    /// <response code="200">Returns the paginated sequence of approved placement templates successfully.</response>
    /// <response code="401">Returned if the request header lacks valid session authentication context bearer tokens.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<ApprovedTemplateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PaginatedResult<ApprovedTemplateDto>>> GetApproved(
        [FromQuery] GetApprovedTemplatesQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}