using AcademicGateway.Api.Common.Models;
using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.ProjectTemplates.Commands.AddGlobalMilestone;
using AcademicGateway.Domain.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.ProjectTemplates.Commands.AddGlobalMilestone;

/// <summary>
/// API Request payload schema for appending a new global milestone node configuration to a blueprint draft.
/// </summary>
/// <param name="Title">The descriptive functional headline assigned to the milestone phase.</param>
/// <param name="Description">The contextual parameters detailing completion conditions and academic target scope.</param>
/// <param name="ExpectedEffortInHours">The nominal estimation metrics mapping work effort constraints.</param>
/// <param name="WbsWeight">The operational work breakdown structure (WBS) weight percentage relative to total project effort.</param>
/// <param name="GradingWeight">The academic grading score weight contribution percentage relative to total project score.</param>
public record AddGlobalMilestoneRequest(
    string Title,
    string Description,
    decimal ExpectedEffortInHours,
    decimal WbsWeight,
    decimal GradingWeight);

/// <summary>
/// Single Action Controller endpoint enabling authenticated corporate industry providers
/// to add structural phase checkpoints into an existing project template lifecycle draft.
/// </summary>
[ApiController]
[Tags("Project Templates")]
[Authorize(Roles = Roles.Provider)] // Restrict modification capability exclusively to industry organization clients
[Route("api/project-templates/{projectTemplateId:guid}/milestones")]
public class AddGlobalMilestoneController(
    ISender mediator,
    ICurrentUserService currentUserService) : ControllerBase
{
    /// <summary>
    /// Appends a new global milestone validation boundary node into the parent template blueprint framework.
    /// </summary>
    /// <param name="projectTemplateId">The tracking identifier key parsing out the targeted parent aggregate layout root.</param>
    /// <param name="request">The structural payload defining the metadata, effort, and independent weight distributions.</param>
    /// <returns>A 201 Created response carrying a strongly-typed contract containing the unique tracking identifier allocated for the milestone.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ResourceCreatedResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddMilestone(
        [FromRoute] Guid projectTemplateId,
        [FromBody] AddGlobalMilestoneRequest request)
    {
        // Guard Invariant: Authenticated provider principal matching must be explicitly satisfied
        if (!currentUserService.UserId.HasValue)
        {
            return Unauthorized("Provider corporate security context could not be resolved.");
        }

        // Hydrate the CQRS command object mapping path route keys alongside body configuration state details
        var command = new AddGlobalMilestoneCommand
        {
            ProjectTemplateId = projectTemplateId,
            Title = request.Title,
            Description = request.Description,
            ExpectedEffortInHours = request.ExpectedEffortInHours,
            WbsWeight = request.WbsWeight,
            GradingWeight = request.GradingWeight
        };

        // Dispatch downstream into the application service pipeline boundary layer via MediatR bus
        var milestoneId = await mediator.Send(command);

        // Stream back standard strongly-typed response tracking resource creation footprint mapping payload
        return Created(string.Empty, new ResourceCreatedResponse(milestoneId));
    }
}