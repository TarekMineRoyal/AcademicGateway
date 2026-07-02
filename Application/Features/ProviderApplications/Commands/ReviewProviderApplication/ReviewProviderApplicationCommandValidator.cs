using FluentValidation;

namespace AcademicGateway.Application.Features.ProviderApplications.Commands.ReviewProviderApplication;

/// <summary>
/// Validates incoming arguments for the <see cref="ReviewProviderApplicationCommand"/> before handler routing occurs.
/// Enforces relational identifier presence and conditional text block constraints for enrollment decision workflows.
/// </summary>
public class ReviewProviderApplicationCommandValidator : AbstractValidator<ReviewProviderApplicationCommand>
{
    /// <summary>
    /// Initializes operational and conditional validation rules for processing corporate provider onboarding evaluations.
    /// </summary>
    public ReviewProviderApplicationCommandValidator()
    {
        // Ensure the tracking application target identifier is specified
        RuleFor(x => x.ApplicationId)
            .NotEmpty().WithMessage("Application ID is required.");

        // Ensure the reviewer domain identity identifier is provided
        // Architectural Optimization: Removed MaximumLength(128) because ReviewerId is now a strongly typed Guid.
        RuleFor(x => x.ReviewerId)
            .NotEmpty().WithMessage("Reviewer ID is required.");

        // Conditional Business Rule: Enforce a rejection reason ONLY when IsApproved is false
        When(x => !x.IsApproved, () =>
        {
            RuleFor(x => x.RejectionReason)
                .NotEmpty()
                .WithMessage("A rejection reason must be provided when declining a corporate provider application.")
                .MinimumLength(10)
                .WithMessage("The rejection reason must be at least 10 characters long to provide actionable feedback to the partner.")
                .MaximumLength(1000)
                .WithMessage("The rejection reason cannot exceed 1000 characters.");
        });
    }
}