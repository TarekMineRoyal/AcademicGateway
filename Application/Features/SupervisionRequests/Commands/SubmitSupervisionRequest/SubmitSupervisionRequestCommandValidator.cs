using FluentValidation;

namespace AcademicGateway.Application.Features.SupervisionRequests.Commands.SubmitSupervisionRequest;

/// <summary>
/// Validates incoming arguments for the <see cref="SubmitSupervisionRequestCommand"/> before handler routing occurs.
/// Enforces metadata consistency and enforces text length requirements on the student's matchmaking pitch.
/// </summary>
public class SubmitSupervisionRequestCommandValidator : AbstractValidator<SubmitSupervisionRequestCommand>
{
    /// <summary>
    /// Initializes validation rule constraints for submitting an academic supervision request.
    /// </summary>
    public SubmitSupervisionRequestCommandValidator()
    {
        RuleFor(x => x.ProjectInstanceId)
            .NotEmpty()
            .WithMessage("Project Instance ID is required to associate the matchmaking request.");

        RuleFor(x => x.ProfessorId)
            .NotEmpty()
            .WithMessage("Target Professor ID is required to route the invitation.");

        RuleFor(x => x.PitchText)
            .NotEmpty()
            .WithMessage("A motivation pitch statement is required.")
            .MinimumLength(20)
            .WithMessage("Your pitch statement must provide at least 20 characters of contextual detail.")
            .MaximumLength(1500)
            .WithMessage("Your pitch statement cannot exceed 1500 characters.");
    }
}