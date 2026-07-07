using FluentValidation;

namespace AcademicGateway.Application.Features.Providers.Commands.UpdateProviderProfile;

/// <summary>
/// Validates incoming arguments for the <see cref="UpdateProviderProfileCommand"/> before handler routing occurs.
/// Enforces business rule constraints, structural length limitations, and basic formatting validations.
/// </summary>
public class UpdateProviderProfileCommandValidator : AbstractValidator<UpdateProviderProfileCommand>
{
    /// <summary>
    /// Initializes standard format filters and string boundary conditions for provider profile updates.
    /// </summary>
    public UpdateProviderProfileCommandValidator()
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("Provider corporate company name cannot be empty or whitespace.")
            .MaximumLength(150).WithMessage("Corporate company name description cannot exceed 150 characters.");

        RuleFor(x => x.CompanyDescription)
            .NotEmpty().WithMessage("Corporate organization operational description summary cannot be empty.")
            .MaximumLength(2000).WithMessage("Corporate description text cannot exceed 2000 characters.");

        RuleFor(x => x.WebsiteUrl)
            .NotEmpty().WithMessage("Official corporate website verification URL cannot be empty or whitespace.")
            .MaximumLength(250).WithMessage("Website verification URL path cannot exceed 250 characters.")
            .Must(url => url != null && (url.StartsWith("http://") || url.StartsWith("https://")))
            .WithMessage("Official corporate website reference must be a valid absolute HTTP or HTTPS URL string.");
    }
}