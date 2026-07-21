using System.Collections.Generic;

namespace AcademicGateway.Application.Common.Models.AiSync;

/// <summary>
/// Wrapper payload command for bulk project template vector index synchronization.
/// Targets POST /api/v1/sync/bulk/project.
/// </summary>
public class BulkSyncProjectCommand
{
    /// <summary>
    /// Gets or sets the collection of project template sync items.
    /// </summary>
    public List<ProjectSyncModel> Items { get; set; } = new();
}