namespace Domain.Common.Enums;

/// <summary>
/// Defines the expected format for a milestone deliverable.
/// </summary>
public enum DeliverableType
{
    /// <summary>
    /// For purely informational, attendance-based, or verified externally by a supervisor milestones.
    /// </summary>
    None = 0,

    /// <summary>
    /// A web-accessible link (e.g., GitHub repository, deployment link).
    /// </summary>
    Url = 1,

    /// <summary>
    /// A document or file upload (e.g., PDF, DOCX, Image).
    /// </summary>
    File = 2,

    /// <summary>
    /// Plain text entry or summary.
    /// </summary>
    Text = 3
}