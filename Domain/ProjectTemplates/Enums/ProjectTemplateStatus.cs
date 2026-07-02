namespace AcademicGateway.Domain.ProjectTemplates.Enums;

/// <summary>
/// Defines the specific state of a project template within its curation and review lifecycle.
/// </summary>
public enum ProjectTemplateStatus
{
    /// <summary>
    /// The template is under construction by the provider and hidden from reviewers.
    /// </summary>
    Draft = 1,

    /// <summary>
    /// The template has been submitted and is currently awaiting evaluation by a reviewer.
    /// </summary>
    PendingReview = 2,

    /// <summary>
    /// A reviewer has noted minor issues and sent the template back to the provider for corrections.
    /// </summary>
    ChangesRequested = 3,

    /// <summary>
    /// A reviewer has modified the template data directly and sent it back to the provider to confirm the edits.
    /// </summary>
    PendingProviderAcceptance = 4,

    /// <summary>
    /// The template has passed all verification gates and is publicly discoverable by students.
    /// </summary>
    Approved = 5,

    /// <summary>
    /// The template violates core platform policies or has been permanently denied.
    /// </summary>
    Rejected = 6,

    /// <summary>
    /// The template was previously approved but has been retired from active use.
    /// </summary>
    Archived = 7
}