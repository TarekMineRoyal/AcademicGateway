using AcademicGateway.Application.Common.Models;
using AcademicGateway.Application.Features.TechSupportProposals.Queries.GetTechSupportProposals;
using AcademicGateway.Domain.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.TechSupportProposals.Queries.GetTechSupportProposals;

/// <summary>
/// Single Action Controller endpoint enabling authorized ecosystem participants to retrieve
/// outstanding corporate industry mentorship offers tied to an active running workspace.
/// </summary>
[ApiController]
[Tags("Tech Support Proposals")]
[Authorize(Roles = $"{Roles.Student},{Roles.Professor}")]
[Route("api/project-instances/{projectInstanceId:guid}/tech-support-proposals")]
public class GetTechSupportProposalsController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Fetches a paginated list of corporate technical assistance offers associated with the specified project workspace channel.
    /// </summary>
    /// <param name="projectInstanceId">The unique lookup identifier key targeting the live ProjectInstance aggregate root.</param>
    /// <param name="pageNumber">The 1-based index of the page to retrieve (default: 1).</param>
    /// <param name="pageSize">The maximum number of items to retrieve per page (default: 10).</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be aborted.</param>
    /// <returns>A 200 OK status containing the paginated collection of tracked proposals, or a 403 Forbidden if ownership tenancy fails verification.</returns>
    /// <response code="200">Returns the paginated sequence of workspace technical assistance offers successfully.</response>
    /// <response code="401">Returned if the request header lacks valid session authentication context bearer tokens.</response>
    /// <response code="403">Returned if the authenticated principal fails framework security role boundary authorization constraints.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<TechSupportProposalDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PaginatedResult<TechSupportProposalDto>>> GetProposals(
        [FromRoute] Guid projectInstanceId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        // Hydrate the CQRS query record using route and query parameters
        var query = new GetTechSupportProposalsQuery(projectInstanceId, pageNumber, pageSize);

        // Dispatch via MediatR bus down into the application secure query execution pipeline
        var result = await mediator.Send(query, cancellationToken);

        // Return the fully populated paginated sequence of workspace technical assistance offers
        return Ok(result);
    }
}