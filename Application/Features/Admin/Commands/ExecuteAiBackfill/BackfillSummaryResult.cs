namespace AcademicGateway.Application.Features.Admin.Commands.ExecuteAiBackfill;

/// <summary>
/// Summary metrics returned upon executing an AI vector matchmaking backfill operation.
/// </summary>
public class BackfillSummaryResult
{
    /// <summary>
    /// Gets or sets the total number of skills synchronized.
    /// </summary>
    public int SkillsSynced { get; set; }

    /// <summary>
    /// Gets or sets the total number of professors synchronized.
    /// </summary>
    public int ProfessorsSynced { get; set; }

    /// <summary>
    /// Gets or sets the total number of students synchronized.
    /// </summary>
    public int StudentsSynced { get; set; }

    /// <summary>
    /// Gets or sets the total number of project templates synchronized.
    /// </summary>
    public int ProjectsSynced { get; set; }

    /// <summary>
    /// Gets or sets the total HTTP batch requests dispatched.
    /// </summary>
    public int TotalBatchesDispatched { get; set; }
}