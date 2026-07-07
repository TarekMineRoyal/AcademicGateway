using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.ProjectTemplates.Commands.ResubmitProjectTemplate;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.ProjectTemplates.Commands.ResubmitProjectTemplate;

/// <summary>
/// API Request payload schema for modifying and resubmitting a rejected or drafted project template blueprint.
/// </summary>
public record ResubmitTemplateRequest(string Title, string Description, List<Guid> SkillIds);

/// <summary>
/// Single Action Controller endpoint allowing authenticated corporate industry providers 
/// to update and resubmit a project template blueprint after an iteration loop pushback.
/// </summary>
[ApiController]
[Tags("Project Templates")]
[Authorize(Roles = "Provider")] // Enforce compliance security context constraints
[Route("api/project-templates")]
public class ResubmitProjectTemplateController(
    ISender mediator,
    ICurrentUserService currentUserService) : ControllerBase
{
    /// <summary>
    /// Modifies and resubmits an existing project template blueprint back into the faculty evaluation pipeline.
    /// </summary>
    /// <param name="id">The unique identifier primary key of the targeted project template blueprint aggregate root.</param>
    /// <param name="request">The payload containing updated title parameters, descriptive scopes, and baseline capability selection requirements.</param>
    /// <returns>A 200 OK response carrying the primary tracking identifier of the updated compliance record.</returns>
    [HttpPut("{id:guid}/resubmit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Resubmit([FromRoute] Guid id, [FromBody] ResubmitTemplateRequest request)
    {
        // Guard Invariant: Validate corporate account authentication states
        if (!currentUserService.UserId.HasValue)
        {
            return Unauthorized("Provider corporate security context could not be resolved.");
        }

        // Hydrate the CQRS command object securely mapping path and body parameters
        var command = new ResubmitProjectTemplateCommand
        {
            Id = id,
            Title = request.Title,
            Description = request.Description,
            SkillIds = request.SkillIds
        };

        var templateId = await mediator.Send(command);

        // Stream back a standardized 200 OK confirmation payload tracking footprint
        return Ok(new { Id = templateId });
    }
}