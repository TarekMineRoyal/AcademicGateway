using AcademicGateway.Application.Features.ProjectTemplates.Commands.DeleteGlobalMilestone;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.ProjectTemplates.Commands.DeleteGlobalMilestone;

/// <summary>
/// Exposes the HTTP endpoint for corporate partners to permanently remove an existing global milestone blueprint node
/// alongside its associated dependency connections from an under-construction project template graph.
/// </summary>
[ApiController]
[Authorize(Roles = "Provider")]
[Route("api/project-templates/{projectTemplateId:guid}/milestones")]
public class DeleteGlobalMilestoneController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Removes a specified global milestone node from a parent project template aggregate root, cascading dependency edge cleanup.
    /// </summary>
    /// <param name="projectTemplateId">The tracking identifier code of the parent template root aggregate.</param>
    /// <param name="milestoneId">The tracking identifier code of the specific milestone blueprint node to delete.</param>
    /// <param name="cancellationToken">A system-managed abort token signaling network transaction lifecycle cancellation changes.</param>
    /// <returns>A 204 NoContent status response code upon a successful state mutation transaction execution boundary.</returns>
    [HttpDelete("{milestoneId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMilestone(
        [FromRoute] Guid projectTemplateId,
        [FromRoute] Guid milestoneId,
        CancellationToken cancellationToken)
    {
        // Synthesize route parameters cleanly into our application command object layout
        var command = new DeleteGlobalMilestoneCommand
        {
            ProjectTemplateId = projectTemplateId,
            MilestoneId = milestoneId
        };

        // Dispatch the deletion request down through the validation pipeline and command execution handler context
        await mediator.Send(command, cancellationToken);

        return NoContent();
    }
}