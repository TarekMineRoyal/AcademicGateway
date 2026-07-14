using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.ProjectInstances.Commands.EvaluateMilestone;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.ProjectInstances.Commands.EvaluateMilestone;

/// <summary>
/// API Request payload schema for recording academic evaluation metrics against a milestone submission.
/// </summary>
/// <param name="Grade">The numerical score awarded by the evaluating faculty member.</param>
/// <param name="Feedback">Optional qualitative analysis, correction summaries, or assessment critique notes.</param>
public record EvaluateMilestoneRequest(decimal Grade, string? Feedback);

/// <summary>
/// Single Action Controller endpoint allowing authenticated academic professors/supervisors 
/// to evaluate, score, and attach critical feedback notes to an active project instance milestone.
/// </summary>
[ApiController]
[Tags("Project Instances")]
[Authorize(Roles = "Professor")] // Enforce that only authorized faculty supervisors can execute evaluation actions
[Route("api/project-instances/{projectInstanceId:guid}/milestones/{localMilestoneId:guid}/evaluation")]
public class EvaluateMilestoneController(
    ISender mediator,
    ICurrentUserService currentUserService) : ControllerBase
{
    /// <summary>
    /// Processes and records a milestone evaluation pass from an assigned academic supervisor.
    /// </summary>
    /// <param name="projectInstanceId">The tracking identifier key parsing out the targeted live project workspace root.</param>
    /// <param name="localMilestoneId">The unique tracking identifier of the target LocalMilestone node being evaluated.</param>
    /// <param name="request">The incoming payload containing the grade metric and descriptive feedback text.</param>
    /// <returns>A 204 No Content response confirming the evaluation state mutation was processed successfully.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EvaluateMilestone(
        [FromRoute] Guid projectInstanceId,
        [FromRoute] Guid localMilestoneId,
        [FromBody] EvaluateMilestoneRequest request)
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