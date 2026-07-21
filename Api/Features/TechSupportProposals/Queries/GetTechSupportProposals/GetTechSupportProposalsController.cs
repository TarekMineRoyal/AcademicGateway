using AcademicGateway.Application.Features.TechSupportProposals.Queries.GetTechSupportProposals;
using AcademicGateway.Domain.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
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
    /// Fetches all corporate technical assistance offers associated with the specified project workspace channel.
    /// </summary>
    /// <param name="projectInstanceId">The unique lookup identifier key targeting the live ProjectInstance aggregate root.</param>
    /// <returns>A 200 OK status containing the collection of tracked proposals, or a 403 Forbidden if ownership tenancy fails verification.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyCollection<TechSupportProposalDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetProposals([FromRoute] Guid projectInstanceId)
    {
        // Hydrate the CQRS query record using the inbound route tracking parameter
        var query = new GetTechSupportProposalsQuery { ProjectInstanceId = projectInstanceId };

        // Dispatch via MediatR bus down into the application secure query execution pipeline
        var proposals = await mediator.Send(query);

        // Return the fully populated sequence of workspace technical assistance offers
        return Ok(proposals);
    }
}