using MediatR;
using System;

namespace AcademicGateway.Application.Features.TechSupportAccounts.Commands.CreateTechSupportAccount;

/// <summary>
/// CQRS Command to provision a new auxiliary external technical support or mentor profile.
/// Creates the underlying security user identity credentials and instantiates a valid TechSupportAccount aggregate record.
/// </summary>
public record CreateTechSupportAccountCommand : IRequest<Guid>
{
    /// <summary>
    /// Gets the unique identifier of the corporate industry provider executing the provisioning transaction.
    /// Maps directly to the parent firm managing this support or mentorship account.
    /// </summary>
    public Guid ProviderId { get; init; }

    /// <summary>
    /// Gets the institutional or corporate email address requested for the newly provisioned support agent account credentials.
    /// This value will serve as both the security username identifier and contact point.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Gets the plain-text password requested for credential configuration.
    /// </summary>
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// Gets the corporate or institutional employee identification number code assigned to this support agent for auditing.
    /// </summary>
    public string StaffNumber { get; init; } = string.Empty;

    /// <summary>
    /// Gets the operational tier or access assignment level designation for this agent (e.g., "Tier 1 Helpdesk", "Mentor").
    /// </summary>
    public string SupportTier { get; init; } = string.Empty;
}