using System.Collections.Generic;

namespace AcademicGateway.Application.Common.Models.AiSync;

/// <summary>
/// Wrapper payload command for bulk professor vector index synchronization.
/// Targets POST /api/v1/sync/bulk/professor.
/// </summary>
public class BulkSyncProfessorCommand
{
    /// <summary>
    /// Gets or sets the collection of professor sync items.
    /// </summary>
    public List<ProfessorSyncModel> Items { get; set; } = new();
}