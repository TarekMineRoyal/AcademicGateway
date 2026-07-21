using AcademicGateway.Application.Features.ProjectInstances.Queries.GetProjectInstanceMilestones;
using AcademicGateway.Domain.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.ProjectInstances.Queries.GetProjectInstanceMilestones;

/// <summary>
/// Single Action Controller endpoint enabling authenticated ecosystem participants to retrieve
/// the complete localized milestone execution matrix graph linked to a live project workspace channel.
/// </summary>
[ApiController]
[Tags("Project Instances")]
[Authorize(Roles = $"{Roles.Student},{Roles.Professor},{Roles.TechSupport}")]
[Route("api/project-instances/{projectInstanceId:guid}/milestones")]
public class GetProjectInstanceMilestonesController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Fetches the sequential task matrix checklist and dependency conditions for a requested project instance workspace.
    /// </summary>
    /// <param name="projectInstanceId">The primary lookup tracking identifier key targeting the parent live ProjectInstance aggregate root.</param>
    /// <returns>A 200 OK status containing the array collection grid of execution milestones.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ProjectInstanceMilestoneDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMilestones([FromRoute] Guid projectInstanceId)
    {
        // Hydrate the CQRS read query object using the inbound route route tracking identifier key
        var query = new GetProjectInstanceMilestonesQuery { ProjectInstanceId = projectInstanceId };

        // Dispatch via MediatR bus down into the application layer to pull flattened, non-tracking data records
        var milestones = await mediator.Send(query);

        // Standard REST collection pattern: return the list matrix envelope safely to the calling client context
        return Ok(milestones);
    }
}