using FluentValidation;
using System;

namespace AcademicGateway.Application.Features.ProjectTemplates.Commands.AddGlobalTask;

/// <summary>
/// Proactively validates incoming data arguments for the <see cref="AddGlobalTaskCommand"/> 
/// before it reaches the MediatR request execution handler pipeline.
/// </summary>
public class AddGlobalTaskCommandValidator : AbstractValidator<AddGlobalTaskCommand>
{
    /// <summary>
    /// Initializes validation constraints for nested template task blueprint additions.
    /// </summary>
    public AddGlobalTaskCommandValidator()
    {
        RuleFor(x => x.ProjectTemplateId)
            .NotEmpty()
            .WithMessage("The Parent Project Template Identifier is mandatory.");

        RuleFor(x => x.GlobalMilestoneId)
            .NotEmpty()
            .WithMessage("The target Global Milestone Identifier container is mandatory.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Task title cannot be empty.")
            .MaximumLength(200)
            .WithMessage("Task title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Task instructions/description must be provided.")
            .MaximumLength(4000)
            .WithMessage("Task description text must not exceed 4000 characters.");

        RuleFor(x => x.Weight)
            .InclusiveBetween(0, 100)
            .WithMessage("Localized task weight percentage must be between 0% and 100% inclusive.");

        RuleFor(x => x.RequiredDeliverableType)
            .IsInEnum()
            .WithMessage("The specified deliverable layout format selection is invalid.");
    }
}