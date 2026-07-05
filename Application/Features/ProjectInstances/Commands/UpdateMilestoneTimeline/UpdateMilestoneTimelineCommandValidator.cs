using FluentValidation;
using System;

namespace AcademicGateway.Application.Features.ProjectInstances.Commands.UpdateMilestoneTimeline;

/// <summary>
/// Enforces formal data sanitation and basic boundary checks for incoming <see cref="UpdateMilestoneTimelineCommand"/> requests.
/// Serves as the initial validation gate in the pipeline before loading the full aggregate root graph into memory.
/// </summary>
public class UpdateMilestoneTimelineCommandValidator : AbstractValidator<UpdateMilestoneTimelineCommand>
{
    /// <summary>
    /// Initializes validation constraints for local milestone timeline adjustments.
    /// </summary>
    public UpdateMilestoneTimelineCommandValidator()
    {
        RuleFor(x => x.ProjectInstanceId)
            .NotEmpty()
            .WithMessage("The Parent Project Instance Identifier context is mandatory.");

        RuleFor(x => x.LocalMilestoneId)
            .NotEmpty()
            .WithMessage("The target Local Milestone Identifier is mandatory.");

        RuleFor(x => x.ScheduledStartDate)
            .NotEmpty()
            .WithMessage("The proposed scheduled start date cannot be empty.");

        RuleFor(x => x.ScheduledEndDate)
            .NotEmpty()
            .WithMessage("The proposed scheduled end date cannot be empty.")
            .GreaterThan(x => x.ScheduledStartDate)
            .WithMessage("Application Layer Guard: The scheduled end date must be strictly later than the scheduled start date.");
    }
}