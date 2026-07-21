using System.Collections.Generic;

namespace AcademicGateway.Application.Common.Models.AiSync;

/// <summary>
/// Wrapper payload command for bulk skill vector index synchronization.
/// Targets POST /api/v1/sync/bulk/skill.
/// </summary>
public class BulkSyncSkillCommand
{
    /// <summary>
    /// Gets or sets the collection of skill sync items.
    /// </summary>
    public List<SkillSyncModel> Items { get; set; } = new();
}