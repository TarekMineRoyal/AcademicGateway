using FluentValidation;

namespace AcademicGateway.Application.Features.ProjectTemplates.Commands.CreateTemplate;

public class CreateProjectTemplateCommandValidator : AbstractValidator<CreateProjectTemplateCommand>
{
    public CreateProjectTemplateCommandValidator()
    {
        // Enforce provider tracking session context presence
        RuleFor(x => x.ProviderId)
            .NotEmpty().WithMessage("Provider ID is required.");

        // Title validation constraints
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Project title is required.")
            .MinimumLength(5).WithMessage("Project title must be at least 5 characters long.")
            .MaximumLength(100).WithMessage("Project title cannot exceed 100 characters.");

        // Description validation constraints (Ensures clarity for reviewing and student matching)
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Project description is required.")
            .MinimumLength(20).WithMessage("Project description must provide at least 20 characters of detail.")
            .MaximumLength(2000).WithMessage("Project description cannot exceed 2000 characters.");

        // Expected Duration constraints adjusted for academic term alignment (e.g., 1 week to 6 months)
        RuleFor(x => x.ExpectedDurationWeeks)
            .GreaterThan(0).WithMessage("Expected duration must be at least 1 week.")
            .LessThanOrEqualTo(26).WithMessage("Expected duration cannot exceed 26 weeks (one academic semester).");

        // Skill requirements validation
        RuleFor(x => x.SkillIds)
            .NotEmpty().WithMessage("At least one required skill must be specified for the project template.")
            .Must(skills => skills != null && skills.Count <= 10).WithMessage("You cannot assign more than 10 required skills to a single template.");
    }
}