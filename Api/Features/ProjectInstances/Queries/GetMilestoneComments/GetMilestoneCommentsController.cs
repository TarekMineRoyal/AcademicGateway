using AcademicGateway.Application.Features.ProjectInstances.Queries.GetMilestoneComments;
using AcademicGateway.Domain.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.ProjectInstances.Queries.GetMilestoneComments;

/// <summary>
/// Single Action Controller endpoint enabling authenticated ecosystem participants to retrieve
/// the chronological collaboration chat feed linked to a specific localized milestone execution leg.
/// </summary>
[ApiController]
[Tags("Project Instances")]
[Authorize(Roles = $"{Roles.Student},{Roles.Professor},{Roles.TechSupport}")]
[Route("api/project-instances/{projectInstanceId:guid}/milestones/{localMilestoneId:guid}/comments")]
public class GetMilestoneCommentsController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Fetches the historical feedback and discussion messages committed inside a targeted milestone workspace channel.
    /// </summary>
    /// <param name="projectInstanceId">The primary lookup tracking identifier key targeting the parent live ProjectInstance aggregate root.</param>
    /// <param name="localMilestoneId">The unique tracking identifier of the localized milestone checkpoint sub-node.</param>
    /// <returns>A 200 OK status containing the array collection stream of collaboration comments.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<MilestoneCommentDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetComments(
        [FromRoute] Guid projectInstanceId,
        [FromRoute] Guid localMilestoneId)
    {
        // Hydrate the CQRS read query object drawing parameters securely from the route tracking identifiers
        var query = new GetMilestoneCommentsQuery
        {
            ProjectInstanceId = projectInstanceId,
            LocalMilestoneId = localMilestoneId
        };

        // Dispatch via MediatR bus down into the application layer to pull sorted, non-tracking message data records
        var comments = await mediator.Send(query);

        // Standard REST collection pattern: return the list envelope safely to the calling client context
        return Ok(comments);
    }
}