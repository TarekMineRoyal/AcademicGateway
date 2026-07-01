using System;

namespace Domain.Providers;

/// <summary>
/// Represents an external technical support or mentor profile provisioned by a Provider.
/// These accounts are managed by corporate partners to oversee student activities, 
/// maintain technical environments, and supervise active project instances.
/// </summary>
public class TechSupportAccount
{
    /// <summary>
    /// Gets the unique identifier for the Tech Support profile. 
    /// Maps 1:1 to the underlying Identity ApplicationUser identifier.
    /// </summary>
    public Guid Id { get; private set; } // Updated to Guid

    /// <summary>
    /// Gets the institutional staff or employee number assigned to this support agent for internal auditing.
    /// </summary>
    public string StaffNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the specific operational tier or access assignment level (e.g., Tier 1 Helpdesk, Tier 3 Systems Admin).
    /// </summary>
    public string SupportTier { get; private set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether this technical support account is authorized to perform active system operations.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// EF Core constructor requirement. Prevents bypass of domain constraints during persistence hydration.
    /// </summary>
    private TechSupportAccount()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TechSupportAccount"/> profile with validation boundaries.
    /// </summary>
    /// <param name="id">The unique Identity key linking back to the account credentials.</param>
    /// <param name="staffNumber">The corporate or institutional employee identifier tracking number.</param>
    /// <param name="supportTier">The assignment tier designation level.</param>
    /// <exception cref="ArgumentException">Thrown when essential tracking values are missing or blank.</exception>
    public TechSupportAccount(Guid id, string staffNumber, string supportTier)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Identity User ID cannot be empty.", nameof(id));
        }

        Id = id;
        UpdateStaffDetails(staffNumber, supportTier);
        IsActive = true; // Accounts are initialized as active by default
    }

    /// <summary>
    /// Updates the core administrative operational tracking parameters for the support staff member.
    /// </summary>
    /// <param name="staffNumber">The updated employee identification string code.</param>
    /// <param name="supportTier">The updated service level tier categorization phrase.</param>
    /// <exception cref="ArgumentException">Thrown if input validation checks fail.</exception>
    public void UpdateStaffDetails(string staffNumber, string supportTier)
    {
        if (string.IsNullOrWhiteSpace(staffNumber))
        {
            throw new ArgumentException("Staff number cannot be empty or whitespace.", nameof(staffNumber));
        }

        if (string.IsNullOrWhiteSpace(supportTier))
        {
            throw new ArgumentException("Support tier assignment level cannot be empty.", nameof(supportTier));
        }

        StaffNumber = staffNumber.Trim();
        SupportTier = supportTier.Trim();
    }

    /// <summary>
    /// Suspends the technical support profile, freezing administrative tool capabilities.
    /// </summary>
    public void DeactivateAccount()
    {
        IsActive = false;
    }

    /// <summary>
    /// Restores active operational capabilities to the technical support profile.
    /// </summary>
    public void ActivateAccount()
    {
        IsActive = true;
    }
}