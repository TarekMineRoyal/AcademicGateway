using AcademicGateway.Application.Features.ProjectTemplates.Commands.UpdateGlobalMilestone;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.ProjectTemplates.Commands.UpdateGlobalMilestone;

/// <summary>
/// Exposes the HTTP endpoint for corporate partners to modify and refine milestone blueprinted criteria
/// inside an under-construction project template configuration graph.
/// </summary>
[ApiController]
[Authorize(Roles = "Provider")]
[Route("api/project-templates/{projectTemplateId:guid}/milestones")]
public class UpdateGlobalMilestoneController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Updates an existing global milestone configuration profile criteria within a parent template boundary container.
    /// </summary>
    /// <param name="projectTemplateId">The tracking identifier code of the parent template root aggregate.</param>
    /// <param name="milestoneId">The tracking identifier code of the target milestone blueprint node to modify.</param>
    /// <param name="request">The request body envelope carrying rewritten textual specification inputs and updated metrics.</param>
    /// <param name="cancellationToken">A system-managed abort token signaling network transaction lifecycle cancellation changes.</param>
    /// <returns>A 204 NoContent status response code upon a successful state mutation transaction execution boundary.</returns>
    [HttpPut("{milestoneId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMilestone(
        [FromRoute] Guid projectTemplateId,
        [FromRoute] Guid milestoneId,
        [FromBody] UpdateGlobalMilestoneRequest request,
        CancellationToken cancellationToken)
    {
        // Synthesize route constraints seamlessly alongside payload metadata elements into a single execution command
        var command = new UpdateGlobalMilestoneCommand
        {
            ProjectTemplateId = projectTemplateId,
            MilestoneId = milestoneId,
            Title = request.Title,
            Description = request.Description,
            ExpectedEffortInHours = request.ExpectedEffortInHours,
            WbsWeight = request.WbsWeight,
            GradingWeight = request.GradingWeight
        };

        // Dispatch the initialized command down through the request pipeline validation behaviors and handlers
        await mediator.Send(command, cancellationToken);

        return NoContent();
    }
}

/// <summary>
/// Defines the specific payload data structure required in the request body envelope to update a milestone's details.
/// </summary>
/// <param name="Title">The newly updated descriptive headline title assigned to the milestone phase.</param>
/// <param name="Description">The revised contextual parameters mapping work item goals and academic scope.</param>
/// <param name="ExpectedEffortInHours">The nominal estimation metrics mapping work effort constraints measured in hours.</param>
/// <param name="WbsWeight">The operational work breakdown structure (WBS) weight percentage relative to the total project effort.</param>
/// <param name="GradingWeight">The academic grading score weight contribution percentage relative to total project score.</param>
public record UpdateGlobalMilestoneRequest(
    string Title,
    string Description,
    decimal ExpectedEffortInHours,
    decimal WbsWeight,
    decimal GradingWeight);