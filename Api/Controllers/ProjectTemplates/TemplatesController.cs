using AcademicGateway.Application.Features.ProjectTemplates.Commands.CreateTemplate;
using AcademicGateway.Application.Features.ProjectTemplates.Queries.GetApprovedTemplates;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Api.Controllers.ProjectTemplates;

/// <summary>
/// Manages the lifecycle of project templates, including creation by providers and retrieval of approved blueprints.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TemplatesController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Drafts a new project curriculum template and registers its required technical skill sets.
    /// </summary>
    /// <param name="request">The template creation details.</param>
    /// <returns>The unique identifier of the created template.</returns>
    [HttpPost]
    [Authorize(Roles = "Provider")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Guid>> Create([FromBody] ApiCreateTemplateRequest request)
    {
        var providerIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(providerIdString, out var providerId))
        {
            return Unauthorized("Provider corporate security context could not be resolved from token metadata.");
        }

        var command = new CreateProjectTemplateCommand
        {
            ProviderId = providerId,
            Title = request.Title,
            Description = request.Description,
            SkillIds = request.SkillIds
        };

        var templateId = await mediator.Send(command);

        // Return 201 Created with the new ID
        return CreatedAtAction(nameof(GetApproved), new { id = templateId }, templateId);
    }

    /// <summary>
    /// Queries the global ecosystem marketplace repository for fully vetted and approved project curricula.
    /// </summary>
    /// <param name="skillId">Optional filter for a specific technical skill requirement.</param>
    /// <returns>A collection of approved project templates.</returns>
    [HttpGet("approved")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ApprovedTemplateDto>>> GetApproved([FromQuery] Guid? skillId)
    {
        var query = new GetApprovedTemplatesQuery
        {
            SkillId = skillId
        };

        var templates = await mediator.Send(query);
        return Ok(templates);
    }
}

/// <summary>
/// DTO for project template creation, isolated from internal domain logic.
/// </summary>
public record ApiCreateTemplateRequest(
    string Title,
    string Description,
    List<Guid> SkillIds);