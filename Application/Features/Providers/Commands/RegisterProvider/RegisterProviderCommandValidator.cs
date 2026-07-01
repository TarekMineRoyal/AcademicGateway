using AcademicGateway.Application.Common.Validations;
using FluentValidation;

namespace AcademicGateway.Application.Features.Providers.Commands.RegisterProvider;

/// <summary>
/// Validates incoming arguments for the <see cref="RegisterProviderCommand"/> before handler routing occurs.
/// Enforces string scale boundaries, password complexity regex structures, and baseline corporate profile constraints.
/// </summary>
public class RegisterProviderCommandValidator : AbstractValidator<RegisterProviderCommand>
{
    /// <summary>
    /// Initializes functional format constraints and identity validation filters for corporate provider registration.
    /// </summary>
    public RegisterProviderCommandValidator()
    {
        // Base Identity Domain Validations (DRY)
        RuleFor(x => x.Email).ValidIdentityEmail();
        RuleFor(x => x.Username).ValidIdentityUsername();
        RuleFor(x => x.Password).ValidIdentityPassword();

        // Corporate Business Rules
        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("Company name is required and cannot be empty.")
            .MaximumLength(100).WithMessage("Company name description records cannot exceed 100 characters.");

        RuleFor(x => x.CompanyDescription)
            .NotEmpty().WithMessage("Company operational background and industry focus details are required.")
            .MaximumLength(2000).WithMessage("Company descriptive profiles cannot exceed 2000 characters.");

        // Conditional Validation: Verify the structural format of the website URI link if supplied
        When(x => !string.IsNullOrEmpty(x.WebsiteUrl), () =>
        {
            RuleFor(x => x.WebsiteUrl)
                .Matches(@"^https?://").WithMessage("Website URL must start with a valid http:// or https:// protocol prefix.")
                .MaximumLength(200).WithMessage("Website link description URL cannot exceed 200 characters.");
        });
    }
}