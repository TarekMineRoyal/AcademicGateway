using FluentValidation;

namespace AcademicGateway.Application.Features.Users.Commands.RegisterProvider;

public class RegisterProviderCommandValidator : AbstractValidator<RegisterProviderCommand>
{
    public RegisterProviderCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Username).NotEmpty().MinimumLength(3);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);

        RuleFor(x => x.OrganizationName).NotEmpty().WithMessage("Organization name is required.");
        RuleFor(x => x.Industry).NotEmpty().WithMessage("Industry is required.");

        // Optional field, but if provided, must be a valid URL
        When(x => !string.IsNullOrEmpty(x.WebsiteUrl), () => {
            RuleFor(x => x.WebsiteUrl).Matches(@"^https?://").WithMessage("Website URL must start with http:// or https://");
        });
    }
}