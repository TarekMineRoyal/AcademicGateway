using AcademicGateway.Api.Common.Models;
using AcademicGateway.Application.Features.SupervisionRequests.Commands.SubmitSupervisionRequest;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.SupervisionRequests.Commands.SubmitSupervisionRequest;

/// <summary>
/// API Request payload schema for submitting a new academic matchmaking supervision invitation to a professor.
/// </summary>
public record SubmitSupervisionRequestRequest(Guid ProfessorId, string PitchText);

/// <summary>
/// Single Action Controller endpoint allowing authenticated student workspace owners to submit a formal 
/// supervision tracking invitation request to an academic professor.
/// </summary>
[Authorize(Roles = "Student")]
[ApiController]
[Tags("Supervision Requests")]
[Route("api/project-instances/{projectInstanceId:guid}/supervision-requests")]
public class SubmitSupervisionRequestController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Submits a formal supervision tracking request to an academic professor for an active workspace runner.
    /// </summary>
    /// <param name="projectInstanceId">The unique tracking identifier of the parent project instance workspace captured from the route segment.</param>
    /// <param name="request">The targeted professor identity details and the motivational pitch composition text.</param>
    /// <param name="cancellationToken">The system thread execution cancellation monitor hook.</param>
    /// <returns>A 201 Created response carrying the strongly typed contract containing the tracking identifier of the newly logged supervision request.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ResourceCreatedResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Submit(
        [FromRoute] Guid projectInstanceId,
        [FromBody] SubmitSupervisionRequestRequest request,
        CancellationToken cancellationToken)
    {
        // Hydrate the CQRS command object mapping parameters from route bounds and the incoming payload cleanly
        var command = new SubmitSupervisionRequestCommand
        {
            ProjectInstanceId = projectInstanceId,
            ProfessorId = request.ProfessorId,
            PitchText = request.PitchText
        };

        var requestId = await mediator.Send(command, cancellationToken);

        // Return a standardized strongly typed contract signaling successful resource creation
        return Created(string.Empty, new ResourceCreatedResponse(requestId));
    }
}