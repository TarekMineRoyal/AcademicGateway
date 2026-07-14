using FluentValidation;
using System;

namespace AcademicGateway.Application.Features.ProjectInstances.Commands.SubmitTaskDeliverable;

/// <summary>
/// Enforces structural data validation and sanitation policies for incoming <see cref="SubmitTaskDeliverableCommand"/> requests.
/// Acts as an early application gatekeeper to block improperly formed payloads before aggregate execution.
/// </summary>
public class SubmitTaskDeliverableCommandValidator : AbstractValidator<SubmitTaskDeliverableCommand>
{
    /// <summary>
    /// Initializes data sanitation and string validation boundaries for student task deliverable submissions.
    /// </summary>
    public SubmitTaskDeliverableCommandValidator()
    {
        RuleFor(x => x.ProjectInstanceId)
            .NotEmpty()
            .WithMessage("The Parent Project Instance Identifier context is mandatory.");

        RuleFor(x => x.LocalMilestoneId)
            .NotEmpty()
            .WithMessage("The destination Local Milestone Identifier is mandatory.");

        RuleFor(x => x.LocalTaskId)
            .NotEmpty()
            .WithMessage("The destination Local Task Identifier is mandatory.");

        RuleFor(x => x.SubmissionPayload)
            .NotEmpty()
            .WithMessage("The work deliverable submission payload cannot be empty or whitespace.")
            .MaximumLength(4000)
            .WithMessage("Application Layer Guard: Submission contents must not exceed the maximum storage cap of 4000 characters.");
    }
}