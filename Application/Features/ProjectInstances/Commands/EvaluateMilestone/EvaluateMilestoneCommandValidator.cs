using FluentValidation;
using System;

namespace AcademicGateway.Application.Features.ProjectInstances.Commands.EvaluateMilestone;

/// <summary>
/// Enforces basic data integrity and boundary sanitation constraints for incoming <see cref="EvaluateMilestoneCommand"/> requests.
/// Acts as an early application pipeline filter before checking specialized domain strategy algorithms.
/// </summary>
public class EvaluateMilestoneCommandValidator : AbstractValidator<EvaluateMilestoneCommand>
{
    /// <summary>
    /// Initializes data structure validation rules for supervisor milestone evaluation passes.
    /// </summary>
    public EvaluateMilestoneCommandValidator()
    {
        RuleFor(x => x.ProjectInstanceId)
            .NotEmpty()
            .WithMessage("The Parent Project Instance Workspace Identifier context is mandatory.");

        RuleFor(x => x.LocalMilestoneId)
            .NotEmpty()
            .WithMessage("The target Local Milestone Identifier code is mandatory.");

        RuleFor(x => x.Grade)
            .InclusiveBetween(0.00m, 100.00m)
            .WithMessage("Application Layer Guard: Raw evaluation scores must reside within the standard base range of 0.00 to 100.00.");

        RuleFor(x => x.Feedback)
            .MaximumLength(4000)
            .WithMessage("Application Layer Guard: Evaluator feedback commentary text must not exceed the maximum storage cap of 4000 characters.");

        RuleFor(x => x.ExecutingProfessorId)
            .NotEmpty()
            .WithMessage("The tracking Identifier code of the grading supervisor is mandatory to enforce authority controls.");
    }
}