using FluentValidation;
using System;

namespace AcademicGateway.Application.Features.ProjectTemplates.Commands.AddGlobalMilestone;

/// <summary>
/// Proactively validates incoming data arguments for the <see cref="AddGlobalMilestoneCommand"/> 
/// before it reaches the MediatR request execution handler pipeline.
/// </summary>
public class AddGlobalMilestoneCommandValidator : AbstractValidator<AddGlobalMilestoneCommand>
{
    /// <summary>
    /// Initializes validation constraints for template milestone blueprint additions.
    /// </summary>
    public AddGlobalMilestoneCommandValidator()
    {
        RuleFor(x => x.ProjectTemplateId)
            .NotEmpty()
            .WithMessage("The Parent Project Template Identifier is mandatory.");

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

        RuleFor(x => x.WbsWeight)
            .InclusiveBetween(0, 100)
            .WithMessage("Operational WBS weight percentage must be between 0% and 100% inclusive.");

        RuleFor(x => x.GradingWeight)
            .InclusiveBetween(0, 100)
            .WithMessage("Academic grading weight percentage must be between 0% and 100% inclusive.");
    }
}