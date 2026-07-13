using FluentValidation;
using System;

namespace AcademicGateway.Application.Features.ProjectTemplates.Commands.DeleteGlobalMilestone;

/// <summary>
/// Proactively validates incoming data arguments for the <see cref="DeleteGlobalMilestoneCommand"/> 
/// before it reaches the MediatR request execution handler pipeline.
/// </summary>
public class DeleteGlobalMilestoneCommandValidator : AbstractValidator<DeleteGlobalMilestoneCommand>
{
    /// <summary>
    /// Initializes validation constraints for template milestone blueprint deletions.
    /// </summary>
    public DeleteGlobalMilestoneCommandValidator()
    {
        RuleFor(x => x.ProjectTemplateId)
            .NotEmpty()
            .WithMessage("The Parent Project Template Identifier is mandatory.");

        RuleFor(x => x.MilestoneId)
            .NotEmpty()
            .WithMessage("The target milestone tracking identifier is mandatory.");
    }
}