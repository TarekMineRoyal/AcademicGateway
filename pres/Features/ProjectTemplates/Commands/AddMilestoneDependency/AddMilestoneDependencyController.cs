using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.ProjectTemplates.Commands.AddMilestoneDependency;
using AcademicGateway.Domain.Common.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.ProjectTemplates.Commands.AddMilestoneDependency;

/// <summary>
/// API Request payload schema for defining a sequencing dependency relationship between two template milestones.
/// </summary>
/// <param name="SuccessorId">The tracking identifier of the milestone node that depends on the predecessor's state execution.</param>
/// <param name="PredecessorId">The tracking identifier of the milestone node that must fulfill criteria or complete first.</param>
/// <param name="Type">The explicit scheduling dependency restriction logic applied to the timeline (e.g., FinishToStart, StartToStart).</param>
public record AddMilestoneDependencyRequest(
    Guid SuccessorId,
    Guid PredecessorId,
    DependencyType Type);

/// <summary>
/// Single Action Controller endpoint enabling authenticated corporate industry providers to establish 
/// scheduling dependencies and sequencing workflows between global milestone nodes within a template blueprint draft.
/// </summary>
[ApiController]
[Tags("Project Templates")]
[Authorize(Roles = "Provider")] // Enforce that only verified corporate owners can configure blueprint graph constraints
[Route("api/project-templates/{projectTemplateId:guid}/dependencies")]
public class AddMilestoneDependencyController(
    ISender mediator,
    ICurrentUserService currentUserService) : ControllerBase
{
    /// <summary>
    /// Connects two template milestones via a directed dependency constraint edge, validating graph loop safety downstream.
    /// </summary>
    /// <param name="projectTemplateId">The primary key identifier of the parent ProjectTemplate aggregate layout root.</param>
    /// <param name="request">The structural model payload matching source nodes, target nodes, and edge constraint behaviors.</param>
    /// <returns>A 204 No Content response signifying successful relationship persistence and acyclic integration checking.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddDependency(
        [FromRoute] Guid projectTemplateId,
        [FromBody] AddMilestoneDependencyRequest request)
    {
        // Guard Invariant: Authenticated provider principal matching must be explicitly satisfied
        if (!currentUserService.UserId.HasValue)
        {
            return Unauthorized("Provider corporate security context could not be resolved.");
        }

        // Hydrate the CQRS command object securely drawing from route values and payload properties
        var command = new AddMilestoneDependencyCommand
        {
            ProjectTemplateId = projectTemplateId,
            SuccessorId = request.SuccessorId,
            PredecessorId = request.PredecessorId,
            Type = request.Type
        };

        // Dispatch downstream into the application service pipeline boundary layer via MediatR bus
        await mediator.Send(command);

        // Standard REST pattern: Return 204 No Content for processing changes carrying no payload entity state
        return NoContent();
    }
}