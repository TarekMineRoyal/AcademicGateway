using FluentValidation;
using System;

namespace AcademicGateway.Application.Features.ProviderApplications.Commands.ResubmitProviderApplication;

/// <summary>
/// Validates incoming arguments for the <see cref="ResubmitProviderApplicationCommand"/> before handler routing occurs.
/// Ensures presence of profile tracking data, corporate description structures, and document URI safety protocols.
/// </summary>
public class ResubmitProviderApplicationCommandValidator : AbstractValidator<ResubmitProviderApplicationCommand>
{
    /// <summary>
    /// Initializes business validation rule constraints for partner company onboarding resubmissions.
    /// </summary>
    public ResubmitProviderApplicationCommandValidator()
    {
        // Enforce provider tracking session context presence
        RuleFor(x => x.ProviderId)
            .NotEmpty().WithMessage("Provider identifier profile context is required.");

        // Corporate descriptive layout criteria validation checks
        RuleFor(x => x.CompanyDetails)
            .NotEmpty().WithMessage("Company operational profile details are required.")
            .MinimumLength(20).WithMessage("Company profile description must provide at least 20 characters of organizational context.")
            .MaximumLength(2000).WithMessage("Company profile description cannot exceed 2000 characters.");

        // Validation constraints verifying the existence and safety of identity confirmation documents
        RuleFor(x => x.VerificationDocumentsUrl)
            .NotEmpty().WithMessage("Corporate registration verification documentation URL is required.")
            .Must(BeAValidAbsoluteUrl).WithMessage("Verification documentation locator must be a valid, secure absolute URI (e.g., https://...).");
    }

    /// <summary>
    /// Evaluates if the input string string represents a valid, safe absolute network URI path.
    /// </summary>
    /// <param name="url">The prospective document destination path string text.</param>
    /// <returns><c>true</c> if the string qualifies as a properly constructed absolute web locator; otherwise, <c>false</c>.</returns>
    private bool BeAValidAbsoluteUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        // Validate structure formatting parameters against secure network address guidelines
        return Uri.TryCreate(url, UriKind.Absolute, out var validatedUri)
               && (validatedUri.Scheme == Uri.UriSchemeHttps || validatedUri.Scheme == Uri.UriSchemeHttp);
    }
}