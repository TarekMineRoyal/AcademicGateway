using AcademicGateway.Application.Common.Models;
using AcademicGateway.Application.Features.SupervisionRequests.Queries.GetPendingSupervisionRequests;
using AcademicGateway.Domain.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.SupervisionRequests.Queries.GetPendingSupervisionRequests;

/// <summary>
/// Single Action Controller endpoint enabling authenticated academic supervisors to retrieve
/// their outstanding, pending matchmaking invitations pipeline.
/// </summary>
[ApiController]
[Tags("Supervision Requests")]
[Authorize(Roles = Roles.Professor)]
[Route("api/supervision-requests/pending/{professorId:guid}")]
public class GetPendingSupervisionRequestsController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Fetches a paginated list of pending supervision requests assigned to the specified professor.
    /// </summary>
    /// <param name="professorId">The unique tracking identifier of the targeted professor profile.</param>
    /// <param name="pageNumber">The 1-based index of the page to retrieve (default: 1).</param>
    /// <param name="pageSize">The maximum number of items to retrieve per page (default: 10).</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be aborted.</param>
    /// <returns>A 200 OK status containing the paginated collection of pending invitations.</returns>
    /// <response code="200">Returns the paginated sequence of pending supervision requests successfully.</response>
    /// <response code="401">Returned if the request header lacks valid session authentication context bearer tokens.</response>
    /// <response code="403">Returned if the authenticated principal fails framework security role boundary authorization constraints.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<PendingSupervisionRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PaginatedResult<PendingSupervisionRequestDto>>> GetPendingRequests(
        [FromRoute] Guid professorId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        // Hydrate the CQRS query record using route and query parameters
        var query = new GetPendingSupervisionRequestsQuery(professorId, pageNumber, pageSize);

        // Dispatch via MediatR bus down into the application secure query execution pipeline
        var result = await mediator.Send(query, cancellationToken);

        // Return the fully populated sequence of pending requests
        return Ok(result);
    }
}