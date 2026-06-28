using AcademicGateway.Application.Features.ProjectTemplates.Commands.CreateTemplate;
using AcademicGateway.Application.Features.ProjectTemplates.Queries.GetApprovedTemplates;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TemplatesController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Drafts a new project curriculum template and maps its required technical skill sets.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Provider")] // Guard: Only registered Provider corporate profiles can draft templates
    public async Task<ActionResult<Guid>> Create([FromBody] ApiCreateTemplateRequest request)
    {
        // Resolve the Provider's primary security key from the authenticated token context
        var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(providerId))
        {
            return Unauthorized("Provider corporate security context could not be resolved from token metadata.");
        }

        var command = new CreateProjectTemplateCommand
        {
            ProviderId = providerId,
            Title = request.Title,
            Description = request.Description,
            ExpectedDurationWeeks = request.ExpectedDurationWeeks,
            SkillIds = request.SkillIds
        };

        var templateId = await mediator.Send(command);
        return CreatedAtAction(nameof(GetApproved), new { }, templateId);
    }

    /// <summary>
    /// Queries the global ecosystem marketplace repository for fully vetted and approved project curricula.
    /// </summary>
    [HttpGet("approved")]
    [Authorize] // Open to all authenticated actors (Students browsing, Providers auditing, Reviewers monitoring)
    public async Task<ActionResult<List<ApprovedTemplateDto>>> GetApproved(
        [FromQuery] int? maxDurationWeeks,
        [FromQuery] Guid? skillId)
    {
        var query = new GetApprovedTemplatesQuery
        {
            MaxDurationWeeks = maxDurationWeeks,
            SkillId = skillId
        };

        var templates = await mediator.Send(query);
        return Ok(templates);
    }
}

// Inline input binding transfer contract to isolate API layer signatures from Application layer models
public record ApiCreateTemplateRequest(
    string Title,
    string Description,
    int ExpectedDurationWeeks,
    List<Guid> SkillIds);