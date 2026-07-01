using System;
using Domain.Common;
using Domain.Providers.Enums;
using Domain.Providers.Events;
using Domain.Providers.Exceptions;
using Domain.SystemStaff;

namespace Domain.Providers;

/// <summary>
/// Represents the stateful onboarding application workflow for an external provider seeking verification within the gateway.
/// </summary>
public class ProviderApplication : BaseEntity
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
    /// <exception cref="InvalidApplicationDetailsException">Thrown when any required parameter constraints fail validation boundaries.</exception>
    public ProviderApplication(Guid providerId, string companyDetails, string verificationDocumentsUrl, DateTime createdAt)
    {
        if (providerId == Guid.Empty)
        {
            throw new InvalidApplicationDetailsException("Provider ID cannot be an empty Guid.");
        }

        if (string.IsNullOrWhiteSpace(companyDetails))
        {
            throw new InvalidApplicationDetailsException("Company details cannot be empty or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(verificationDocumentsUrl))
        {
            throw new InvalidApplicationDetailsException("Verification documents URL cannot be empty or whitespace.");
        }

        Id = Guid.NewGuid();
        ProviderId = providerId;
        CompanyDetails = companyDetails.Trim();
        VerificationDocumentsUrl = verificationDocumentsUrl.Trim();
        Status = ProviderApplicationStatus.Draft;
        CreatedAt = createdAt;
    }

    /// <summary>
    /// Transitions the application into the evaluation pool.
    /// </summary>
    /// <exception cref="InvalidApplicationStatusException">Thrown if execution is attempted outside the Draft state context.</exception>
    public void SubmitForReview()
    {
        if (Status != ProviderApplicationStatus.Draft)
        {
            throw new InvalidApplicationStatusException(Status, nameof(SubmitForReview));
        }

        Status = ProviderApplicationStatus.PendingReview;
    }

    /// <summary>
    /// Overwrites a previously rejected application with updated files and text, shifting the 
    /// existing record back into the pending review pool.
    /// </summary>
    /// <param name="newCompanyDetails">The corrected descriptive overview of the company profile.</param>
    /// <param name="newVerificationDocumentsUrl">The new reference URI locating corporate verification paperwork.</param>
    /// <exception cref="InvalidApplicationStatusException">Thrown if executed outside of a Rejected state context.</exception>
    /// <exception cref="InvalidApplicationDetailsException">Thrown if input validation constraints fail validation boundaries.</exception>
    public void Resubmit(string newCompanyDetails, string newVerificationDocumentsUrl)
    {
        if (Status != ProviderApplicationStatus.Rejected)
        {
            throw new InvalidApplicationStatusException(Status, nameof(Resubmit));
        }

        if (string.IsNullOrWhiteSpace(newCompanyDetails))
        {
            throw new InvalidApplicationDetailsException("Company details cannot be empty or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(newVerificationDocumentsUrl))
        {
            throw new InvalidApplicationDetailsException("Verification documents URL cannot be empty or whitespace.");
        }

        CompanyDetails = newCompanyDetails.Trim();
        VerificationDocumentsUrl = newVerificationDocumentsUrl.Trim();
        Status = ProviderApplicationStatus.PendingReview;

        ReviewedById = null;
        ReviewedAt = null;
        RejectionReason = null;
    }

    /// <summary>
    /// Commits an approval transition, clearing legacy rejections, storing structural audit references,
    /// and raising a domain event to handle multi-aggregate side effects cleanly.
    /// </summary>
    /// <param name="reviewerId">The identifier code tracking the certifying reviewer.</param>
    /// <param name="approvedAt">The deterministic timestamp when the approval is signed off.</param>
    /// <exception cref="InvalidApplicationDetailsException">Thrown when reviewer key validation boundaries fail.</exception>
    /// <exception cref="InvalidApplicationStatusException">Thrown if execution is attempted outside the PendingReview state context.</exception>
    public void Approve(Guid reviewerId, DateTime approvedAt)
    {
        if (Status != ProviderApplicationStatus.PendingReview)
        {
            throw new InvalidApplicationStatusException(Status, nameof(Approve));
        }

        if (reviewerId == Guid.Empty)
        {
            throw new InvalidApplicationDetailsException("A valid reviewer ID must be provided to approve an application.");
        }

        if (approvedAt < CreatedAt)
        {
            throw new InvalidApplicationDetailsException("Approval date cannot be older than the application creation date.");
        }

        Status = ProviderApplicationStatus.Approved;
        ReviewedById = reviewerId;
        RejectionReason = null;
        ReviewedAt = approvedAt;

        AddDomainEvent(new ProviderApplicationApprovedEvent(ProviderId));
    }

    /// <summary>
    /// Commits a rejection transition, requiring an explanatory comment for the corporate contact.
    /// </summary>
    /// <param name="reviewerId">The identifier code tracking the evaluating reviewer.</param>
    /// <param name="reason">The explanation framing why the verification paperwork was deemed insufficient.</param>
    /// <param name="rejectedAt">The deterministic timestamp when the rejection is signed off.</param>
    /// <exception cref="InvalidApplicationDetailsException">Thrown if reason text parameters or reviewer identifiers fail criteria checks.</exception>
    /// <exception cref="InvalidApplicationStatusException">Thrown if execution is attempted outside the PendingReview state context.</exception>
    public void Reject(Guid reviewerId, string reason, DateTime rejectedAt)
    {
        if (Status != ProviderApplicationStatus.PendingReview)
        {
            throw new InvalidApplicationStatusException(Status, nameof(Reject));
        }

        if (reviewerId == Guid.Empty)
        {
            throw new InvalidApplicationDetailsException("A valid reviewer ID must be provided to reject an application.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidApplicationDetailsException("A rejection reason must be provided.");
        }

        if (rejectedAt < CreatedAt)
        {
            throw new InvalidApplicationDetailsException("Rejection date cannot be older than the application creation date.");
        }

        Status = ProviderApplicationStatus.Rejected;
        ReviewedById = reviewerId;
        RejectionReason = reason.Trim();
        ReviewedAt = rejectedAt;
    }
}