using AcademicGateway.Application.Features.ProjectTemplates.Queries.GetPendingProjectTemplates;
using AcademicGateway.Domain.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

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
    /// Retrieves a chronological queue of all industry-proposed project templates currently awaiting structural clearance.
    /// </summary>
    /// <param name="cancellationToken">Propagates notification that network operations should be aborted.</param>
    /// <returns>A read-only sequence containing lightweight representation metadata of pending project templates.</returns>
    /// <response code="200">Returns the matching sequence array of pending validation templates successfully.</response>
    /// <response code="401">Returned if the request header lacks valid session authentication context bearer tokens.</response>
    /// <response code="403">Returned if the authenticated principal fails framework security role boundary authorization constraints.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<PendingProjectTemplateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyCollection<PendingProjectTemplateDto>>> GetPendingQueue(
        CancellationToken cancellationToken)
    {
        // Instantiates the isolated read query model and routes via the MediatR memory bus
        var query = new GetPendingProjectTemplatesQuery();
        var result = await mediator.Send(query, cancellationToken);

        return Ok(result);
    }
}