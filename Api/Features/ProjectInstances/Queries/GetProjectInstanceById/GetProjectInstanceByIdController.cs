using AcademicGateway.Application.Features.ProjectInstances.Queries.GetProjectInstanceById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.ProjectInstances.Queries.GetProjectInstanceById;

/// <summary>
/// Single Action Controller endpoint enabling authenticated ecosystem participants to retrieve
/// the complete current runtime execution state, metadata snapshot, and profile bindings of a live project workspace.
/// </summary>
[ApiController]
[Tags("Project Instances")]
[Authorize(Roles = "Student,Professor,TechSupport")]
[Route("api/project-instances/{projectInstanceId:guid}")]
public class GetProjectInstanceByIdController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Fetches the live administrative, tracking, and capability metrics for a requested project instance workspace.
    /// </summary>
    /// <param name="projectInstanceId">The primary lookup identifier key targeting the live ProjectInstance aggregate root workspace.</param>
    /// <returns>A 200 OK status containing the granular runtime snapshot payload, or a 404 Not Found if missing.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProjectInstanceDetailDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInstanceById([FromRoute] Guid projectInstanceId)
    {
        // Hydrate the CQRS query record parameter directly using the inbound route tracking identifier key
        var query = new GetProjectInstanceByIdQuery { Id = projectInstanceId };

        // Dispatch via MediatR bus down into the application layer to grab our non-tracking projection model
        var instanceDetails = await mediator.Send(query);

        // Guard Invariant: If the database projection returns null, bubble back a clean RESTful 404 code
        if (instanceDetails == null)
        {
            return NotFound($"Project instance workspace with ID '{projectInstanceId}' could not be located.");
        }

        // Return the fully populated snapshot data transfer object
        return Ok(instanceDetails);
    }
}