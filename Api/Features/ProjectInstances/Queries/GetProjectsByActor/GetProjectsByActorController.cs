using AcademicGateway.Application.Features.ProjectInstances.Queries.GetProjectsByActor;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.ProjectInstances.Queries.GetProjectsByActor;

/// <summary>
/// Single Action Controller endpoint enabling authenticated ecosystem participants to retrieve
/// a unified, role-based overview dashboard of their associated running project workspace channels.
/// </summary>
[ApiController]
[Tags("Project Instances")]
[Authorize(Roles = "Student,Professor,Provider")]
[Route("api/project-instances/actor/{actorId:guid}")]
public class GetProjectsByActorController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Fetches a dynamic collection of running project workspace summary slots matching the target actor identity and requested role context.
    /// </summary>
    /// <param name="actorId">The primary lookup identifier key targeting the active ecosystem user profile.</param>
    /// <param name="role">The system authorization string role evaluating the contextual data pipeline filters (Student, Professor, or Provider).</param>
    /// <returns>A 200 OK status containing the read-only sequence of dashboard summary rows, or a 403 Forbidden if authorization fails verification.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyCollection<ActorProjectDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetProjectsByActor([FromRoute] Guid actorId, [FromQuery] string role)
    {
        // Hydrate the CQRS query container using the inbound route tracking identifier and query parameters
        var query = new GetProjectsByActorQuery
        {
            ActorId = actorId,
            Role = role
        };

        // Dispatch via MediatR bus down into the application secure dashboard compilation layer
        var projects = await mediator.Send(query);

        // Return the fully populated row sequence representation matching the user workspace landscape
        return Ok(projects);
    }
}