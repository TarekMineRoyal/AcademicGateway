using AcademicGateway.Application.Common.Validations;
using FluentValidation;

namespace AcademicGateway.Application.Features.Users.Commands.RegisterProvider;

public class RegisterProviderCommandValidator : AbstractValidator<RegisterProviderCommand>
{
    public RegisterProviderCommandValidator()
    {
        RuleFor(x => x.Email).ValidIdentityEmail();
        RuleFor(x => x.Username).ValidIdentityUsername();
        RuleFor(x => x.Password).ValidIdentityPassword();

        // Provider-specific logic
        RuleFor(x => x.OrganizationName)
            .NotEmpty().WithMessage("Organization name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Industry)
            .NotEmpty().WithMessage("Industry is required.")
            .MaximumLength(50);

        When(x => !string.IsNullOrEmpty(x.WebsiteUrl), () => {
            RuleFor(x => x.WebsiteUrl)
                .Matches(@"^https?://").WithMessage("Website URL must start with http:// or https://")
                .MaximumLength(200);
        });
    }
}