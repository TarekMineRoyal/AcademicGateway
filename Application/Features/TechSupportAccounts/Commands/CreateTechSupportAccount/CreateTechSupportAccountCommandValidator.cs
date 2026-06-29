using AcademicGateway.Application.Common.Validations;
using FluentValidation;

namespace AcademicGateway.Application.Features.TechSupportAccounts.Commands.CreateTechSupportAccount;

public class CreateTechSupportAccountCommandValidator : AbstractValidator<CreateTechSupportAccountCommand>
{
    public CreateTechSupportAccountCommandValidator()
    {
        // Ensure the managing provider authorization context is supplied
        RuleFor(x => x.ProviderId)
            .NotEmpty().WithMessage("Provider ID is required.")
            .MaximumLength(128).WithMessage("Provider ID cannot exceed 128 characters.");

        // Identity bounds using our DRY Extension Methods
        RuleFor(x => x.Email).ValidIdentityEmail();
        RuleFor(x => x.Password).ValidIdentityPassword();

        // Profile metadata formatting constraints
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MinimumLength(3).WithMessage("Full name must be at least 3 characters long.")
            .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters.");
    }
}