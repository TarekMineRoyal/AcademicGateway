using AcademicGateway.Api.Common.Models;
using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.ProjectTemplates.Commands.CreateProjectTemplate;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.ProjectTemplates.Commands.CreateProjectTemplate;

/// <summary>
/// API Request payload schema for initializing a new reusable project blueprint template draft.
/// </summary>
public record CreateTemplateRequest(string Title, string Description, List<Guid> SkillIds);

/// <summary>
/// Single Action Controller endpoint allowing authenticated corporate industry providers 
/// to construct a reusable project template blueprint draft.
/// </summary>
[ApiController]
[Tags("Project Templates")]
[Authorize(Roles = "Provider")] // Enforce that a valid corporate provider security context exists
[Route("api/project-templates")]
public class CreateProjectTemplateController(
    ISender mediator,
    ICurrentUserService currentUserService) : ControllerBase
{
    /// <summary>
    /// Initializes a brand new reusable project template snapshot blueprint draft.
    /// </summary>
    /// <param name="request">The incoming title details, descriptor scopes, and baseline skill selection array.</param>
    /// <returns>A 201 Created response carrying a strongly-typed contract containing the unique tracking identifier of the initialized blueprint aggregate.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ResourceCreatedResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateTemplateRequest request)
    {
        // Guard Invariant: Validate corporate account authentication states
        if (!currentUserService.UserId.HasValue)
        {
            return Unauthorized("Provider corporate security context could not be resolved.");
        }

        // Hydrate the application CQRS command object securely mapping the token context values
        var command = new CreateProjectTemplateCommand
        {
            ProviderId = currentUserService.UserId.Value,
            Title = request.Title,
            Description = request.Description,
            SkillIds = request.SkillIds
        };

        var templateId = await mediator.Send(command);

        // Stream back a standardized strongly-typed resource tracking response
        return Created(string.Empty, new ResourceCreatedResponse(templateId));
    }
}