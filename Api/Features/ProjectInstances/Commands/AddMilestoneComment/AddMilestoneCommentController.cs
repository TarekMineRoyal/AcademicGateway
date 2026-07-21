using AcademicGateway.Api.Common.Models;
using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.ProjectInstances.Commands.AddMilestoneComment;
using Application.Features.ProjectInstances.Commands.AddMilestoneComment;
using AcademicGateway.Domain.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.ProjectInstances.Commands.AddMilestoneComment;

/// <summary>
/// API Request payload schema for appending a communication record or feedback entry to a milestone timeline.
/// </summary>
/// <param name="Content">The raw text content or markdown markdown payload comprising the body of the message comment.</param>
public record AddMilestoneCommentRequest(string Content);

/// <summary>
/// Single Action Controller endpoint allowing authenticated workspace participants (Students, Professors, Tech Support)
/// to publish collaboration notes and feedback discussion entries directly onto a live project milestone timeline.
/// </summary>
[ApiController]
[Tags("Project Instances")]
[Authorize(Roles = $"{Roles.Student},{Roles.Professor},{Roles.TechSupport}")]
[Route("api/project-instances/{projectInstanceId:guid}/milestones/{localMilestoneId:guid}/comments")]
public class AddMilestoneCommentController(
    ISender mediator,
    ICurrentUserService currentUserService) : ControllerBase
{
    /// <summary>
    /// Appends a new collaboration comment record directly into the specified milestone timeline workspace context.
    /// </summary>
    /// <param name="projectInstanceId">The tracking identifier key parsing out the active target project instance aggregate layer.</param>
    /// <param name="localMilestoneId">The specific localization node key identifying the runtime phase boundary being commented on.</param>
    /// <param name="request">The request body payload wrapping the text content configuration parameters.</param>
    /// <returns>A 201 Created response carrying a strongly-typed contract containing the unique tracking identifier allocated for the milestone comment.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ResourceCreatedResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddComment(
        [FromRoute] Guid projectInstanceId,
        [FromRoute] Guid localMilestoneId,
        [FromBody] AddMilestoneCommentRequest request)
    {
        // Guard Invariant: Ensure the calling principal context is authenticated and resolvable
        if (!currentUserService.UserId.HasValue)
        {
            return Unauthorized("User execution context could not be parsed.");
        }

        // Extract the identity role claim token representation to compile the structural identity snapshot metadata
        string identitySnapshot = User.FindFirstValue(ClaimTypes.Role) ?? "Unknown Participant";

        // Hydrate the CQRS command pattern object, binding path parameters alongside internal identity context tokens
        var command = new AddMilestoneCommentCommand
        {
            ProjectInstanceId = projectInstanceId,
            LocalMilestoneId = localMilestoneId,
            AuthorId = currentUserService.UserId.Value,
            AuthorIdentitySnapshot = identitySnapshot,
            Content = request.Content
        };

        // Dispatch via MediatR bus down into the application processing boundary pipelines
        var commentId = await mediator.Send(command);

        // Stream back standard strongly-typed response tracking resource creation footprint mapping payload
        return Created(string.Empty, new ResourceCreatedResponse(commentId));
    }
}