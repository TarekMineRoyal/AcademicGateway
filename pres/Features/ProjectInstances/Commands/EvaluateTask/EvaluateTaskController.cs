using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.ProjectInstances.Commands.EvaluateTask;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.ProjectInstances.Commands.EvaluateTask;

/// <summary>
/// API Request payload schema for recording academic evaluation metrics against a nested task submission.
/// </summary>
/// <param name="Grade">The numerical score awarded by the evaluating faculty member conforming to active grading strategy boundaries.</param>
/// <param name="Feedback">Optional qualitative analysis, correction summaries, or assessment critique notes.</param>
public record EvaluateTaskRequest(decimal Grade, string? Feedback);

/// <summary>
/// Single Action Controller endpoint enabling authenticated academic professors/supervisors 
/// to evaluate, score, and attach critical feedback notes to an active localized task node execution.
/// </summary>
[ApiController]
[Tags("Project Instances")]
[Authorize(Roles = "Professor")] // Enforce that only authorized faculty supervisors can execute evaluation actions
[Route("api/project-instances/{projectInstanceId:guid}/milestones/{localMilestoneId:guid}/tasks/{localTaskId:guid}/evaluation")]
public class EvaluateTaskController(
    ISender mediator,
    ICurrentUserService currentUserService) : ControllerBase
{
    /// <summary>
    /// Processes and records a task evaluation checkpoint pass from an assigned academic supervisor.
    /// </summary>
    /// <param name="projectInstanceId">The tracking identifier key parsing out the targeted live project workspace root.</param>
    /// <param name="localMilestoneId">The unique tracking identifier of the parent milestone container node.</param>
    /// <param name="localTaskId">The localized task tracking code target node being evaluated.</param>
    /// <param name="request">The incoming payload containing the specific score metric and descriptive feedback text attributes.</param>
    /// <returns>A 204 No Content response confirming the evaluation state mutation was processed successfully.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EvaluateTask(
        [FromRoute] Guid projectInstanceId,
        [FromRoute] Guid localMilestoneId,
        [FromRoute] Guid localTaskId,
        [FromBody] EvaluateTaskRequest request)
    {
        // Guard Invariant: Ensure the supervisor's execution security context is valid and fully resolved
        if (!currentUserService.UserId.HasValue)
        {
            return Unauthorized("Professor evaluation security context could not be resolved.");
        }

        // Hydrate the CQRS command object mapping path route keys alongside body parameters and user identifiers
        var command = new EvaluateTaskCommand
        {
            ProjectInstanceId = projectInstanceId,
            LocalMilestoneId = localMilestoneId,
            LocalTaskId = localTaskId,
            Grade = request.Grade,
            Feedback = request.Feedback,
            ExecutingProfessorId = currentUserService.UserId.Value
        };

        // Dispatch downstream into the application service pipeline boundary layer via MediatR bus
        await mediator.Send(command);

        // Standard REST pattern: Return 204 No Content for state-mutations carrying no entity body downstream
        return NoContent();
    }
}