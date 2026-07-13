using FluentValidation;
using System;

namespace AcademicGateway.Application.Features.ProjectTemplates.Commands.UpdateGlobalMilestone;

/// <summary>
/// Proactively validates incoming data arguments for the <see cref="UpdateGlobalMilestoneCommand"/> 
/// before it reaches the MediatR request execution handler pipeline.
/// </summary>
public class UpdateGlobalMilestoneCommandValidator : AbstractValidator<UpdateGlobalMilestoneCommand>
{
    /// <summary>
    /// Initializes validation constraints for template milestone blueprint updates.
    /// </summary>
    public UpdateGlobalMilestoneCommandValidator()
    {
        RuleFor(x => x.ProjectTemplateId)
            .NotEmpty()
            .WithMessage("The Parent Project Template Identifier is mandatory.");

        RuleFor(x => x.MilestoneId)
            .NotEmpty()
            .WithMessage("The target milestone tracking identifier is mandatory.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Milestone title cannot be empty.")
            .MaximumLength(200)
            .WithMessage("Milestone title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Milestone instructions/description must be provided.")
            .MaximumLength(4000)
            .WithMessage("Milestone description text must not exceed 4000 characters.");

        RuleFor(x => x.ExpectedEffortInHours)
            .GreaterThan(0)
            .WithMessage("Expected milestone effort estimation must be strictly greater than zero hours.");

        RuleFor(x => x.RequiredDeliverableType)
            .IsInEnum()
            .WithMessage("The specified deliverable layout format selection is invalid.");
    }
}