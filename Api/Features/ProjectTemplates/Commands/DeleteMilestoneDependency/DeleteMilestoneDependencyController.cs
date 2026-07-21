using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.ProjectTemplates.Commands.DeleteMilestoneDependency;
using AcademicGateway.Domain.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.ProjectTemplates.Commands.DeleteMilestoneDependency;

/// <summary>
/// API Request payload schema for identifying an existing sequencing dependency relationship between two template milestones to sever.
/// </summary>
/// <param name="SuccessorId">The tracking identifier of the milestone node carrying the inbound restriction edge.</param>
/// <param name="PredecessorId">The tracking identifier of the milestone node representing the prerequisite boundary.</param>
public record DeleteMilestoneDependencyRequest(
    Guid SuccessorId,
    Guid PredecessorId);

/// <summary>
/// Single Action Controller endpoint enabling authenticated corporate industry providers to remove 
/// scheduling dependencies and sever sequencing constraints between global milestone nodes within a template blueprint draft.
/// </summary>
[ApiController]
[Tags("Project Templates")]
[Authorize(Roles = Roles.Provider)] // Enforce that only verified corporate owners can configure blueprint graph constraints
[Route("api/project-templates/{projectTemplateId:guid}/dependencies")]
public class DeleteMilestoneDependencyController(
    ISender mediator,
    ICurrentUserService currentUserService) : ControllerBase
{
    /// <summary>
    /// Severs an active milestone dependency sequencing constraint link separating two milestone nodes within a template graph.
    /// </summary>
    /// <param name="projectTemplateId">The primary key identifier of the parent ProjectTemplate aggregate layout root.</param>
    /// <param name="request">The structural model payload targeting the exact edge constraint connection to disconnect.</param>
    /// <returns>A 204 No Content response signifying successful relationship removal and graph tracking adjustment.</returns>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDependency(
        [FromRoute] Guid projectTemplateId,
        [FromBody] DeleteMilestoneDependencyRequest request)
    {
        // Guard Invariant: Authenticated provider principal matching must be explicitly satisfied
        if (!currentUserService.UserId.HasValue)
        {
            return Unauthorized("Provider corporate security context could not be resolved.");
        }

        // Hydrate the CQRS command object securely drawing from route values and payload properties
        var command = new DeleteMilestoneDependencyCommand
        {
            ProjectTemplateId = projectTemplateId,
            SuccessorId = request.SuccessorId,
            PredecessorId = request.PredecessorId
        };

        // Dispatch downstream into the application service pipeline boundary layer via MediatR bus
        await mediator.Send(command);

        // Standard REST pattern: Return 204 No Content for processing changes carrying no payload entity state
        return NoContent();
    }
}