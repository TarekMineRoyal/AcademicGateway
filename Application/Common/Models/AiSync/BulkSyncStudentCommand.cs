using System.Collections.Generic;

namespace AcademicGateway.Application.Common.Models.AiSync;

/// <summary>
/// Wrapper payload command for bulk student vector index synchronization.
/// Targets POST /api/v1/sync/bulk/student.
/// </summary>
public class BulkSyncStudentCommand
{
    /// <summary>
    /// Gets or sets the collection of student sync items.
    /// </summary>
    public List<StudentSyncModel> Items { get; set; } = new();
}