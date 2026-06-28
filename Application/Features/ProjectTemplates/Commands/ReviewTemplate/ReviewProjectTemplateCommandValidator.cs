using FluentValidation;

namespace AcademicGateway.Application.Features.ProjectTemplates.Commands.ReviewTemplate;

public class ReviewProjectTemplateCommandValidator : AbstractValidator<ReviewProjectTemplateCommand>
{
    public ReviewProjectTemplateCommandValidator()
    {
        // Ensure the targeted template identifier is provided
        RuleFor(x => x.TemplateId)
            .NotEmpty().WithMessage("Template ID is required.");

        // Ensure the identity user context parameter is present
        RuleFor(x => x.ReviewerIdentityUserId)
            .NotEmpty().WithMessage("Reviewer Identity User ID is required.");

        // Conditional Business Rule: Require a rejection reason only when IsApproved is false
        RuleFor(x => x.RejectionReason)
            .NotEmpty()
            .When(x => !x.IsApproved)
            .WithMessage("A rejection reason must be provided when rejecting a project template.")

            .MinimumLength(10)
            .When(x => !x.IsApproved)
            .WithMessage("The rejection reason must be at least 10 characters long to provide actionable feedback.");
    }
}