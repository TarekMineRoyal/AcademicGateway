using AcademicGateway.Application.Features.SupervisionRequests.Queries.GetPendingSupervisionRequests;
using AcademicGateway.Domain.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
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
    /// Fetches all pending supervision requests assigned to the specified professor.
    /// </summary>
    /// <param name="professorId">The unique tracking identifier of the targeted professor profile.</param>
    /// <returns>A 200 OK status containing the read-only collection of pending invitations.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyCollection<PendingSupervisionRequestDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPendingRequests([FromRoute] Guid professorId)
    {
        // Hydrate the CQRS query record using the inbound route tracking parameter
        var query = new GetPendingSupervisionRequestsQuery { ProfessorId = professorId };

        // Dispatch via MediatR bus down into the application secure query execution pipeline
        var requests = await mediator.Send(query);

        // Return the fully populated sequence of pending requests
        return Ok(requests);
    }
}