using AcademicGateway.Application.Common.Validations;
using FluentValidation;

namespace AcademicGateway.Application.Features.TechSupportAccounts.Commands.CreateTechSupportAccount;

/// <summary>
/// Validates incoming arguments for the <see cref="CreateTechSupportAccountCommand"/> before handler routing occurs.
/// Enforces metadata formatting boundaries and standard identity credential restrictions.
/// </summary>
public class CreateTechSupportAccountCommandValidator : AbstractValidator<CreateTechSupportAccountCommand>
{
    /// <summary>
    /// Initializes business and validation rule constraints for auxiliary technical support account provisioning.
    /// </summary>
    public CreateTechSupportAccountCommandValidator()
    {
        // Ensure the managing provider authorization context is supplied
        // Architectural Optimization: Removed MaximumLength(128) because ProviderId is a strongly typed Guid.
        RuleFor(x => x.ProviderId)
            .NotEmpty().WithMessage("Provider ID is required.");

        // Identity bounds using our DRY Extension Methods
        RuleFor(x => x.Email).ValidIdentityEmail();
        RuleFor(x => x.Password).ValidIdentityPassword();

        // Profile metadata formatting constraints matching core domain aggregate rules
        RuleFor(x => x.StaffNumber)
            .NotEmpty().WithMessage("Staff number code is required.")
            .MaximumLength(50).WithMessage("Staff number cannot exceed 50 characters.");

        RuleFor(x => x.SupportTier)
            .NotEmpty().WithMessage("Support tier assignment level is required.")
            .MaximumLength(50).WithMessage("Support tier cannot exceed 50 characters.");
    }
}