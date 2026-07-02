using FluentValidation;

namespace AcademicGateway.Application.Features.ProjectInstances.Commands.StartProject;

/// <summary>
/// Enforces business validation rules for incoming <see cref="StartProjectCommand"/> requests.
/// </summary>
public class StartProjectCommandValidator : AbstractValidator<StartProjectCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StartProjectCommandValidator"/> class.
    /// </summary>
    public StartProjectCommandValidator()
    {
        RuleFor(v => v.TemplateId)
            .NotEmpty()
            .WithMessage("Source Project Template ID is required.");

        RuleFor(v => v.StudentId)
            .NotEmpty()
            .WithMessage("Student ID is required.");

        // Rule for optional requested professor: if specified, it must be a valid, non-empty Guid
        RuleFor(v => v.RequestedProfessorId)
            .NotEqual(System.Guid.Empty)
            .When(v => v.RequestedProfessorId.HasValue)
            .WithMessage("The requested academic supervisor ID cannot be an empty Guid.");
    }
}