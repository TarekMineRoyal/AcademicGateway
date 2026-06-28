using FluentValidation;

namespace AcademicGateway.Application.Features.TechSupportAccounts.Commands.CreateTechSupportAccount;

public class CreateTechSupportAccountCommandValidator : AbstractValidator<CreateTechSupportAccountCommand>
{
    public CreateTechSupportAccountCommandValidator()
    {
        // Ensure the managing provider authorization context is supplied
        RuleFor(x => x.ProviderId)
            .NotEmpty().WithMessage("Provider ID is required.");

        // Enforce formal communication channel syntax for login requirements
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("A valid corporate email address format is required.");

        // Core identity password defense parameters
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one numeric digit.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

        // Profile metadata formatting constraints
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MinimumLength(3).WithMessage("Full name must be at least 3 characters long.")
            .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters.");
    }
}