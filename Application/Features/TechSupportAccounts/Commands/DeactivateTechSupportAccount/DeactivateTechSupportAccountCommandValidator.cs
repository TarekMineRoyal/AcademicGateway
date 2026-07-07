using FluentValidation;
using System;

namespace AcademicGateway.Application.Features.TechSupportAccounts.Commands.DeactivateTechSupportAccount;

/// <summary>
/// Validates incoming arguments for the <see cref="DeactivateTechSupportAccountCommand"/> before handler routing occurs.
/// Enforces request formatting constraints and structural identity presence boundaries.
/// </summary>
public class DeactivateTechSupportAccountCommandValidator : AbstractValidator<DeactivateTechSupportAccountCommand>
{
    /// <summary>
    /// Initializes baseline format filters and structural presence constraints for account deactivation.
    /// </summary>
    public DeactivateTechSupportAccountCommandValidator()
    {
        // Enforce structural tracking resource identity protection rules
        RuleFor(x => x.TechSupportAccountId)
            .NotEmpty().WithMessage("Target tech support account tracking identity reference context cannot be empty.")
            .NotEqual(Guid.Empty).WithMessage("Provided tech support account unique token must evaluate to a valid formatted identifier.");
    }
}