using AcademicGateway.Application.Features.ProviderApplications.Queries.GetProviderApplicationById;
using AcademicGateway.Domain.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.ProviderApplications.Queries.GetProviderApplicationById;

/// <summary>
/// Exposes endpoints for retrieving comprehensive provider application records by identifier.
/// </summary>
[ApiController]
[Tags("Provider Applications")]
[Authorize(Roles = Roles.Reviewer)]
[Route("api/provider-applications")]
public class GetProviderApplicationByIdController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Fetches full provider application details including attached verification documents, credentials, and evaluation history.
    /// </summary>
    /// <param name="id">The primary tracking key identifying the targeted ProviderApplication.</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be aborted.</param>
    /// <returns>A 200 OK status containing full provider application details, or 404 Not Found if missing.</returns>
    /// <response code="200">Returns the matching provider application detail payload.</response>
    /// <response code="401">Returned if the request header lacks valid session authentication bearer tokens.</response>
    /// <response code="403">Returned if the user lacks authorization to access this application resource.</response>
    /// <response code="404">Returned if no application matching the supplied ID exists.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProviderApplicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var query = new GetProviderApplicationByIdQuery { Id = id };
        var result = await mediator.Send(query, cancellationToken);

        if (result == null)
        {
            return NotFound($"Provider application with ID '{id}' could not be located.");
        }

        return Ok(result);
    }
}