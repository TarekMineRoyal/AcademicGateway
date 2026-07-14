using AcademicGateway.Application.Features.ProjectTemplates.Commands.AddGlobalTask;
using AcademicGateway.Domain.Common.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.ProjectTemplates.Commands.AddGlobalTask;

/// <summary>
/// API Request payload schema for building out a new nested blueprint task configuration within a parent milestone container.
/// </summary>
/// <param name="Title">The descriptive functional headline assigned to the nested task blueprint.</param>
/// <param name="Description">The detailed text description outlining the requirements and expectations for the task.</param>
/// <param name="Weight">The operational weight percentage assigned to this task node relative to its parent milestone container.</param>
/// <param name="RequiredDeliverableType">The expected submission format constraint mandated for student work execution under this task blueprint.</param>
public record AddGlobalTaskRequest(
    string Title,
    string Description,
    decimal Weight,
    DeliverableType RequiredDeliverableType);

/// <summary>
/// Single Action Controller endpoint enabling authenticated corporate industry providers
/// to add nested structural task validation items into a blueprinted milestone phase checkpoint.
/// </summary>
[ApiController]
[Tags("Project Templates")]
[Authorize(Roles = "Provider")] // Restrict configuration updates exclusively to industry organization clients
[Route("api/project-templates/{projectTemplateId:guid}/milestones/{globalMilestoneId:guid}/tasks")]
public class AddGlobalTaskController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Creates a newborn nested task configuration node under a specified global milestone phase container.
    /// </summary>
    /// <param name="projectTemplateId">The tracking identifier key identifying the targeted parent aggregate blueprint layout root.</param>
    /// <param name="globalMilestoneId">The tracking identifier code of the specific milestone phase container receiving the new task.</param>
    /// <param name="request">The request body model holding data constraints and deliverable expectations for the new task blueprint.</param>
    /// <returns>A 201 Created response delivering the unique database tracking tracking code of the generated task.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddTask(
        [FromRoute] Guid projectTemplateId,
        [FromRoute] Guid globalMilestoneId,
        [FromBody] AddGlobalTaskRequest request)
    {
        // Hydrate the CQRS command object, linking path route keys seamlessly alongside inbound body criteria
        var command = new AddGlobalTaskCommand
        {
            ProjectTemplateId = projectTemplateId,
            GlobalMilestoneId = globalMilestoneId,
            Title = request.Title,
            Description = request.Description,
            Weight = request.Weight,
            RequiredDeliverableType = request.RequiredDeliverableType
        };

        // Dispatch downstream into the application service pipeline boundary layer via MediatR bus
        var taskId = await mediator.Send(command);

        // Stream back standard RESTful 201 footprint containing the newly allocated task tracking primary key
        return Created(string.Empty, new { Id = taskId });
    }
}