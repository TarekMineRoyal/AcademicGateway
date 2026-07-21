using AcademicGateway.Application.Features.Admin.Commands.ExecuteAiBackfill;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.Admin.Commands.AiSync;

/// <summary>
/// Presentation layer query options for triggering an AI matchmaking vector index backfill operation.
/// </summary>
public record ExecuteAiBackfillRequest(
    bool SyncSkills = true,
    bool SyncProfessors = true,
    bool SyncStudents = true,
    bool SyncProjects = true,
    int ChunkSize = 250);

/// <summary>
/// Administrative controller for triggering bulk data vector index re-synchronization with the AI Matchmaking Engine.
/// </summary>
[ApiController]
[Tags("Admin")]
[Authorize(Roles = "Admin")]
[Route("api/v1/admin/ai-sync")]
public class AiSyncController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Triggers an on-demand bulk backfill operation, querying database records and streaming them in batches to the AI Matchmaking Engine.
    /// </summary>
    /// <param name="request">Optional filter query flags and batch size bounds for the backfill routine.</param>
    /// <returns>A 200 OK response containing summary metrics of queued items and dispatched HTTP batches.</returns>
    [HttpPost("backfill")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BackfillSummaryResult))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExecuteBackfill([FromQuery] ExecuteAiBackfillRequest request)
    {
        var command = new ExecuteAiBackfillCommand
        {
            SyncSkills = request.SyncSkills,
            SyncProfessors = request.SyncProfessors,
            SyncStudents = request.SyncStudents,
            SyncProjects = request.SyncProjects,
            ChunkSize = request.ChunkSize
        };

        var summary = await mediator.Send(command);

        return Ok(summary);
    }
}