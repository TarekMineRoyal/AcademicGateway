using MediatR;
using System;

namespace AcademicGateway.Application.Features.TechSupportAccounts.Commands.DeactivateTechSupportAccount;

/// <summary>
/// CQRS Command to securely deactivate an external tech support or technician account.
/// Allows corporate providers to off-board technicians who change positions or leave the organization.
/// </summary>
public record DeactivateTechSupportAccountCommand : IRequest
{
    /// <summary>
    /// Gets the unique identifier of the target tech support account undergoing deactivation.
    /// </summary>
    public Guid TechSupportAccountId { get; init; }
}