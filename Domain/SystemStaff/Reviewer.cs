using System;
using System.Collections.Generic;
using Domain.Common;
using Domain.Providers;

namespace Domain.SystemStaff;

/// <summary>
/// Represents an internal quality assurance evaluator, administrator, or faculty auditor 
/// responsible for evaluating provider onboarding applications and auditing platform workflows.
/// </summary>
public class Reviewer : BaseEntity
{
    private readonly List<ProviderApplication> _reviewedApplications = new();

    /// <summary>
    /// Gets the unique identifier for the Reviewer profile. 
    /// Maps 1:1 to the underlying Identity ApplicationUser identifier.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the formal full legal or professional name of the reviewer.
    /// </summary>
    public string FullName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the read-only audit historical collection of provider applications processed or evaluated by this reviewer.
    /// </summary>
    public IReadOnlyCollection<ProviderApplication> ReviewedApplications => _reviewedApplications.AsReadOnly();

    /// <summary>
    /// EF Core constructor requirement. Prevents bypass of standard domain constraints during persistence hydration.
    /// </summary>
    private Reviewer() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Reviewer"/> profile with structural tracking parameters.
    /// </summary>
    /// <param name="id">The unique Identity key linking back to the account credentials.</param>
    /// <param name="fullName">The verified professional full name of the evaluator.</param>
    /// <exception cref="ArgumentException">Thrown when validation constraints fail.</exception>
    public Reviewer(Guid id, string fullName)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Identity User ID reference cannot be empty.", nameof(id));
        }

        Id = id;
        UpdateFullName(fullName);
    }

    /// <summary>
    /// Updates the formal professional display identity name of the reviewer.
    /// </summary>
    /// <param name="newFullName">The updated name payload string.</param>
    /// <exception cref="ArgumentException">Thrown if the name argument configuration breaches validation checks.</exception>
    public void UpdateFullName(string newFullName)
    {
        if (string.IsNullOrWhiteSpace(newFullName))
        {
            throw new ArgumentException("Reviewer full name cannot be empty or whitespace.", nameof(newFullName));
        }

        FullName = newFullName.Trim();
    }
}