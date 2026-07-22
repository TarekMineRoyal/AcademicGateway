using AcademicGateway.Application.Common.Models;
using AcademicGateway.Application.Features.ProjectTemplates.Queries.GetPendingProjectTemplates;
using AcademicGateway.Domain.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.ProjectTemplates.Queries.GetPendingProjectTemplates;

/// <summary>
/// Exposes administrative endpoints for managing and auditing industry project blueprint clearance queues.
/// </summary>
[ApiController]
[Authorize(Roles = Roles.Reviewer)]
[Route("api/project-templates/pending")]
public class GetPendingProjectTemplatesController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Retrieves a paginated queue of all industry-proposed project templates currently awaiting structural clearance.
    /// </summary>
    /// <param name="query">The pagination parameters including page number and page size limit.</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be aborted.</param>
    /// <returns>A paginated result containing lightweight representation metadata of pending project templates.</returns>
    /// <response code="200">Returns the paginated sequence of pending validation templates successfully.</response>
    /// <response code="401">Returned if the request header lacks valid session authentication context bearer tokens.</response>
    /// <response code="403">Returned if the authenticated principal fails framework security role boundary authorization constraints.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<PendingProjectTemplateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PaginatedResult<PendingProjectTemplateDto>>> GetPendingQueue(
        [FromQuery] GetPendingProjectTemplatesQuery query,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}