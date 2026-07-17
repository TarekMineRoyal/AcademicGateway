using AcademicGateway.Application.Features.ProviderApplications.Queries.GetPendingProviderApplications;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace AcademicGateway.Api.Features.ProviderApplications.Queries.GetPendingProviderApplications;

/// <summary>
/// Exposes administrative endpoints for managing and auditing provider onboarding application queues.
/// </summary>
[ApiController]
[Authorize(Roles = "Reviewer")]
[Route("api/provider-applications/pending")]
public class GetPendingProviderApplicationsController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Retrieves a chronological queue of all submitted corporate provider applications awaiting institutional verification.
    /// </summary>
    /// <param name="cancellationToken">Propagates notification that network operations should be aborted.</param>
    /// <returns>A read-only sequence containing lightweight representation metadata of pending application payloads.</returns>
    /// <response code="200">Returns the matching sequence array of pending validation records successfully.</response>
    /// <response code="401">Returned if the request header lacks valid session authentication context bearer tokens.</response>
    /// <response code="403">Returned if the authenticated principal fails framework security role boundary authorization constraints.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<PendingProviderApplicationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyCollection<PendingProviderApplicationDto>>> GetPendingQueue(
        CancellationToken cancellationToken)
    {
        // Instantiates the isolated read command model context and routes via the MediatR memory bus
        var query = new GetPendingProviderApplicationsQuery();
        var result = await mediator.Send(query, cancellationToken);

        return Ok(result);
    }
}