using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.ProjectInstances.Commands.StartProject;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.ProjectInstances.Commands.StartProject;

/// <summary>
/// API Request payload schema for initializing a new live project instance workspace from a blueprint template.
/// </summary>
public record StartProjectRequest(Guid TemplateId, Guid? RequestedProfessorId);

/// <summary>
/// Single Action Controller endpoint allowing authenticated students to spin up a live operational workspace copy 
/// from an approved project template blueprint using the Prototype Pattern.
/// </summary>
[Authorize(Roles = "Student")] // Restrict gateway mapping entry strictly to Student roles
[ApiController]
[Tags("Project Instances")]
[Route("api/project-instances")]
public class StartProjectController(ISender mediator, ICurrentUserService currentUserService) : ControllerBase
{
    /// <summary>
    /// Initializes a brand new running project snapshot tracking workspace boundary.
    /// </summary>
    /// <param name="request">The incoming template identification and optional supervisor assignment selection details.</param>
    /// <param name="cancellationToken">The system thread execution cancellation monitor hook.</param>
    /// <returns>A 201 Created response carrying the primary tracking identifier of the newly instantiated workspace.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Start([FromBody] StartProjectRequest request, CancellationToken cancellationToken)
    {
        // Guard Invariant: Safe type resolution checking to avoid null pointer assignment tracking failures
        if (!currentUserService.UserId.HasValue)
        {
            return Unauthorized();
        }

        // Construct the application command, safely mapping the guaranteed non-nullable student GUID token value
        var command = new StartProjectCommand
        {
            TemplateId = request.TemplateId,
            RequestedProfessorId = request.RequestedProfessorId,
            StudentId = currentUserService.UserId.Value
        };

        var projectInstanceId = await mediator.Send(command, cancellationToken);

        // Stream back a standardized RESTful success tracking footprint payload
        return StatusCode(StatusCodes.Status201Created, new
        {
            ProjectInstanceId = projectInstanceId,
            Message = "Project workspace initialized successfully from the template blueprint."
        });
    }
}