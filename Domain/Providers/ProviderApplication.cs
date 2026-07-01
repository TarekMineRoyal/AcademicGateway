using System;
using Domain.Providers.Enums;
using Domain.SystemStaff;

namespace Domain.Providers;

/// <summary>
/// Represents the stateful onboarding application workflow for an external provider seeking verification within the gateway.
/// </summary>
public class ProviderApplication
{
    /// <summary>
    /// Gets the unique identifier for the provider application.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the unique identifier of the associated provider (maps to the ApplicationUser ID).
    /// </summary>
    public Guid ProviderId { get; private set; }

    /// <summary>
    /// Gets the detailed metadata and operational description of the provider's firm or organization.
    /// </summary>
    public string CompanyDetails { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the secure remote storage location link containing verification files.
    /// </summary>
    public string VerificationDocumentsUrl { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the current state within the evaluation pipeline.
    /// </summary>
    public ProviderApplicationStatus Status { get; private set; }

    /// <summary>
    /// Gets the identifier of the reviewer processing this application, if evaluated.
    /// </summary>
    public Guid? ReviewedById { get; private set; }

    /// <summary>
    /// Gets the professional justification context if the application state transitions to Rejected.
    /// </summary>
    public string? RejectionReason { get; private set; }

    /// <summary>
    /// Gets the immutable timestamp indicating when the registration request record was spawned.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Gets the timestamp indicating exactly when the review state change was committed.
    /// </summary>
    public DateTime? ReviewedAt { get; private set; }

    /// <summary>
    /// Gets the navigation property for the creating partner profile.
    /// </summary>
    public Provider Provider { get; private set; } = null!;

    /// <summary>
    /// Gets the navigation property for the assigned quality assurance reviewer profile.
    /// </summary>
    public Reviewer? ReviewedBy { get; private set; }

    /// <summary>
    /// EF Core constructor requirement. Prevents bypass of standard state constraints during persistence hydration.
    /// </summary>
    private ProviderApplication() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderApplication"/> workflow tracking model in Draft mode.
    /// </summary>
    /// <param name="providerId">The identity tracker code mapping back to the account profile.</param>
    /// <param name="companyDetails">The introductory overview specifications of the company profile.</param>
    /// <param name="verificationDocumentsUrl">The reference URI locating corporate verification paperwork.</param>
    /// <param name="createdAt">The deterministic timestamp when this application transaction is initiated.</param>
    /// <exception cref="ArgumentException">Thrown when any required parameter constraints fail validation boundaries.</exception>
    public ProviderApplication(Guid providerId, string companyDetails, string verificationDocumentsUrl, DateTime createdAt)
    {
        if (providerId == Guid.Empty)
        {
            throw new ArgumentException("Provider ID cannot be an empty Guid.", nameof(providerId));
        }

        if (string.IsNullOrWhiteSpace(companyDetails))
        {
            throw new ArgumentException("Company details cannot be empty or whitespace.", nameof(companyDetails));
        }

        if (string.IsNullOrWhiteSpace(verificationDocumentsUrl))
        {
            throw new ArgumentException("Verification documents URL cannot be empty or whitespace.", nameof(verificationDocumentsUrl));
        }

        Id = Guid.NewGuid();
        ProviderId = providerId;
        CompanyDetails = companyDetails.Trim();
        VerificationDocumentsUrl = verificationDocumentsUrl.Trim();
        Status = ProviderApplicationStatus.Draft;

        // Clock state is passed explicitly rather than calling DateTime.UtcNow internally
        CreatedAt = createdAt;
    }

    /// <summary>
    /// Transitions the application into the evaluation pool.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if execution is attempted outside the Draft state context.</exception>
    public void SubmitForReview()
    {
        if (Status != ProviderApplicationStatus.Draft)
        {
            throw new InvalidOperationException("Only draft applications can be submitted for review.");
        }

        Status = ProviderApplicationStatus.PendingReview;
    }

    /// <summary>
    /// Commits an approval transition, clearing legacy rejections and storing structural audit references.
    /// </summary>
    /// <param name="reviewerId">The identifier code tracking the certifying reviewer.</param>
    /// <param name="approvedAt">The deterministic timestamp when the approval is signed off.</param>
    /// <exception cref="ArgumentException">Thrown when reviewer key validation boundaries fail.</exception>
    /// <exception cref="InvalidOperationException">Thrown if execution is attempted outside the PendingReview state context.</exception>
    public void Approve(Guid reviewerId, DateTime approvedAt)
    {
        if (Status != ProviderApplicationStatus.PendingReview)
        {
            throw new InvalidOperationException("Only pending applications can be approved.");
        }

        if (reviewerId == Guid.Empty)
        {
            throw new ArgumentException("A valid reviewer ID must be provided to approve an application.", nameof(reviewerId));
        }

        if (approvedAt < CreatedAt)
        {
            throw new ArgumentException("Approval date cannot be older than the application creation date.", nameof(approvedAt));
        }

        Status = ProviderApplicationStatus.Approved;
        ReviewedById = reviewerId;
        RejectionReason = null;

        // Provided explicitly via method argument
        ReviewedAt = approvedAt;
    }

    /// <summary>
    /// Commits a rejection transition, requiring an explanatory comment for the corporate contact.
    /// </summary>
    /// <param name="reviewerId">The identifier code tracking the evaluating reviewer.</param>
    /// <param name="reason">The explanation framing why the verification paperwork was deemed insufficient.</param>
    /// <param name="rejectedAt">The deterministic timestamp when the rejection is signed off.</param>
    /// <exception cref="ArgumentException">Thrown if reason text parameters or reviewer identifiers fail criteria checks.</exception>
    /// <exception cref="InvalidOperationException">Thrown if execution is attempted outside the PendingReview state context.</exception>
    public void Reject(Guid reviewerId, string reason, DateTime rejectedAt)
    {
        if (Status != ProviderApplicationStatus.PendingReview)
        {
            throw new InvalidOperationException("Only pending applications can be rejected.");
        }

        if (reviewerId == Guid.Empty)
        {
            throw new ArgumentException("A valid reviewer ID must be provided to reject an application.", nameof(reviewerId));
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("A rejection reason must be provided.", nameof(reason));
        }

        if (rejectedAt < CreatedAt)
        {
            throw new ArgumentException("Rejection date cannot be older than the application creation date.", nameof(rejectedAt));
        }

        Status = ProviderApplicationStatus.Rejected;
        ReviewedById = reviewerId;
        RejectionReason = reason.Trim();

        // Provided explicitly via method argument
        ReviewedAt = rejectedAt;
    }
}