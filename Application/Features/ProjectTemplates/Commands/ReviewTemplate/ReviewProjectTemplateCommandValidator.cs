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
            .NotEmpty().WithMessage("Reviewer Identity User ID is required.")
            .MaximumLength(128).WithMessage("Reviewer Identity User ID cannot exceed 128 characters.");

        // Conditional Business Rule: Require a rejection reason only when IsApproved is false
        When(x => !x.IsApproved, () =>
        {
            RuleFor(x => x.RejectionReason)
                .NotEmpty()
                .WithMessage("A rejection reason must be provided when rejecting a project template.")
                .MinimumLength(10)
                .WithMessage("The rejection reason must be at least 10 characters long to provide actionable feedback.")
                .MaximumLength(1000)
                .WithMessage("The rejection reason cannot exceed 1000 characters.");
        });
    }
}