using FluentValidation;

namespace AcademicGateway.Application.Features.ProjectTemplates.Commands.ReviewTemplate;

/// <summary>
/// Validates incoming arguments for the <see cref="ReviewProjectTemplateCommand"/> before handler routing occurs.
/// Enforces relational identifier presence and conditional validation text blocks for action audits.
/// </summary>
public class ReviewProjectTemplateCommandValidator : AbstractValidator<ReviewProjectTemplateCommand>
{
    /// <summary>
    /// Initializes operational and conditional validation rules for processing template evaluations.
    /// </summary>
    public ReviewProjectTemplateCommandValidator()
    {
        // Ensure the targeted template identifier is provided
        RuleFor(x => x.TemplateId)
            .NotEmpty().WithMessage("Template ID is required.");

        // Ensure the reviewer domain identity identifier is provided
        // Architectural Optimization: Removed MaximumLength(128) because ReviewerId is now a strongly typed Guid.
        RuleFor(x => x.ReviewerId)
            .NotEmpty().WithMessage("Reviewer ID is required.");

        // Conditional Business Rule: Require an evaluation reason only when IsApproved is false
        When(x => !x.IsApproved, () =>
        {
            RuleFor(x => x.RejectionReason)
                .NotEmpty()
                .WithMessage("A rejection or modification reason must be provided when declining a project template.")
                .MinimumLength(10)
                .WithMessage("The feedback reason must be at least 10 characters long to provide actionable guidance to the provider.")
                .MaximumLength(1000)
                .WithMessage("The feedback reason cannot exceed 1000 characters.");
        });
    }
}