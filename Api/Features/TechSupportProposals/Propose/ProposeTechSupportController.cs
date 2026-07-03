using AcademicGateway.Application.Features.TechSupportProposals.Commands.ProposeTechSupport;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.TechSupportProposals.Propose;

/// <summary>
/// API Request payload schema for an industry provider to propose a technical support mentor configuration assignment.
/// </summary>
public record ProposeTechSupportRequest(Guid TechSupportAccountId, string Message);

/// <summary>
/// Single Action Controller endpoint allowing authenticated corporate industry providers to assign or propose 
/// a specific technical support profile account to mentor a running student project workspace runner.
/// </summary>
[Authorize(Roles = "Provider")] // Enforce role safety boundaries to protect corporate support assignment channels
[ApiController]
[Route("api/project-instances/{projectInstanceId:guid}/tech-support-proposals")]
public class ProposeTechSupportController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Proposes an explicitly spawned corporate tech support profile to act as a technical mentor to a running project instance.
    /// </summary>
    /// <param name="projectInstanceId">The unique tracking identifier of the student project instance workspace captured from the route segment.</param>
    /// <param name="request">The technical support account identity token and the mandatory contextual introductory introductory message payload.</param>
    /// <param name="cancellationToken">The system thread execution cancellation monitor hook.</param>
    /// <returns>A 201 Created response carrying the primary tracking key of the newly generated mentorship proposal.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Propose(
        [FromRoute] Guid projectInstanceId,
        [FromBody] ProposeTechSupportRequest request,
        CancellationToken cancellationToken)
    {
        // Hydrate the CQRS command object, mapping parameters from the route path context and the inbound payload cleanly
        var command = new ProposeTechSupportCommand
        {
            ProjectInstanceId = projectInstanceId,
            TechSupportAccountId = request.TechSupportAccountId,
            Message = request.Message
        };

        var proposalId = await mediator.Send(command, cancellationToken);

        // Stream back a standardized 201 Created RESTful success tracking footprint payload
        return StatusCode(StatusCodes.Status201Created, new
        {
            TechSupportProposalId = proposalId,
            Message = "Technical support mentorship proposal submitted successfully to the student project workspace owner."
        });
    }
}