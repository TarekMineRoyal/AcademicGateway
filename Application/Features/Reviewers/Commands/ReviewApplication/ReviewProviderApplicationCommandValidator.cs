using FluentValidation;

namespace AcademicGateway.Application.Features.Reviewers.Commands.ReviewApplication;

public class ReviewProviderApplicationCommandValidator : AbstractValidator<ReviewProviderApplicationCommand>
{
    public ReviewProviderApplicationCommandValidator()
    {
        // Ensure the tracking application target identifier is specified
        RuleFor(x => x.ApplicationId)
            .NotEmpty().WithMessage("Application ID is required.");

        // Ensure the security payload tracking context identifier is specified
        RuleFor(x => x.ReviewerIdentityUserId)
            .NotEmpty().WithMessage("Reviewer Identity User ID is required.");

        // Conditional Business Rule: Enforce a rejection reason ONLY when IsApproved is false
        RuleFor(x => x.RejectionReason)
            .NotEmpty()
            .When(x => !x.IsApproved)
            .WithMessage("A rejection reason must be provided when rejecting an application.")

            .MinimumLength(10)
            .When(x => !x.IsApproved)
            .WithMessage("The rejection reason must be at least 10 characters long to provide actionable feedback.");
    }
}