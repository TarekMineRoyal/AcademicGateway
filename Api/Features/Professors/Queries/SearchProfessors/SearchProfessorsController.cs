using AcademicGateway.Application.Common.Models;
using AcademicGateway.Application.Features.Professors.Queries.SearchProfessors;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.Professors.Queries.SearchProfessors;

/// <summary>
/// Endpoint for discovering institutional faculty member accounts.
/// </summary>
[Authorize]
[ApiController]
[Tags("Professors")]
[Route("api/professors")]
public class SearchProfessorsController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Searches across available professor security accounts using partial text matching across full names, emails, or usernames with pagination.
    /// </summary>
    /// <param name="query">The query envelope containing search criteria and pagination options.</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be aborted.</param>
    /// <returns>A paginated sequence containing lightweight presentational professor lookup records.</returns>
    /// <response code="200">Returns the paginated sequence of matching professor search records successfully.</response>
    /// <response code="401">Returned if the request header lacks valid session authentication context bearer tokens.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<ProfessorSearchResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PaginatedResult<ProfessorSearchResultDto>>> SearchProfessors(
        [FromQuery] SearchProfessorsQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}