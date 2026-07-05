using AcademicGateway.Application.Features.ProjectInstances.Commands.SubmitMilestoneDeliverable;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.ProjectInstances.Commands.SubmitMilestoneDeliverable;

/// <summary>
/// API Request payload schema for submitting academic work deliverable artifacts.
/// </summary>
/// <param name="SubmissionPayload">The raw polymorphic payload containing the delivery details (e.g., repository links, text summaries, cloud storage hashes).</param>
public record SubmitMilestoneDeliverableRequest(string SubmissionPayload);

/// <summary>
/// Single Action Controller endpoint allowing authenticated students to submit completion materials 
/// and target executable milestone phases within an assigned project workspace instance.
/// </summary>
[ApiController]
[Tags("Project Instances")]
[Authorize(Roles = "Student")] // Restricted exclusively to student profiles executing their workspace milestones
[Route("api/project-instances/{projectInstanceId:guid}/milestones/{localMilestoneId:guid}/deliverables")]
public class SubmitMilestoneDeliverableController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Commits an academic or practical deliverable payload entry targeting a localized project milestone.
    /// </summary>
    /// <param name="projectInstanceId">The tracking identifier key parsing out the targeted live project workspace root.</param>
    /// <param name="localMilestoneId">The localization checkpoint identifier node receiving the submission entry.</param>
    /// <param name="request">The incoming body model holding data constraints and payload addresses representing completed work.</param>
    /// <returns>A 204 No Content response signifying successful registration and triggering downstream domain format evaluations.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmitDeliverable(
        [FromRoute] Guid projectInstanceId,
        [FromRoute] Guid localMilestoneId,
        [FromBody] SubmitMilestoneDeliverableRequest request)
    {
        // Hydrate the CQRS command pattern, weaving route-based mapping properties securely alongside body attributes
        var command = new SubmitMilestoneDeliverableCommand
        {
            ProjectInstanceId = projectInstanceId,
            LocalMilestoneId = localMilestoneId,
            SubmissionPayload = request.SubmissionPayload
        };

        // Dispatch via MediatR pipeline into application processing boundaries for rule validation mechanics
        await mediator.Send(command);

        // Return a standard RESTful 204 No Content response indicating successful processing with no entity response body
        return NoContent();
    }
}