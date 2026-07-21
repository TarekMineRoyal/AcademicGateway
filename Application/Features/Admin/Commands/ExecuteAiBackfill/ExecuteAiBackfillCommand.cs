using MediatR;

namespace AcademicGateway.Application.Features.Admin.Commands.ExecuteAiBackfill;

/// <summary>
/// CQRS Command to execute an on-demand bulk backfill synchronization pipeline to the AI Matchmaking Engine.
/// Fetches existing active database records and streams them in batches to update vector indices.
/// </summary>
public record ExecuteAiBackfillCommand : IRequest<BackfillSummaryResult>
{
    /// <summary>
    /// Gets a value indicating whether global skills should be synchronized. Defaults to true.
    /// </summary>
    public bool SyncSkills { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether professor profiles should be synchronized. Defaults to true.
    /// </summary>
    public bool SyncProfessors { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether student profiles should be synchronized. Defaults to true.
    /// </summary>
    public bool SyncStudents { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether approved project templates should be synchronized. Defaults to true.
    /// </summary>
    public bool SyncProjects { get; init; } = true;

    /// <summary>
    /// Gets the maximum batch chunk size for outgoing HTTP sync payloads. Defaults to 250.
    /// </summary>
    public int ChunkSize { get; init; } = 250;
}