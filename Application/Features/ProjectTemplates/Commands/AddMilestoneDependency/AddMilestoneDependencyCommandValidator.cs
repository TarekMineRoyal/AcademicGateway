using FluentValidation;
using System;

namespace AcademicGateway.Application.Features.ProjectTemplates.Commands.AddMilestoneDependency;

/// <summary>
/// Proactively validates incoming graph structural inputs for the <see cref="AddMilestoneDependencyCommand"/> 
/// before triggering recursive cycle-detection routines within the aggregate root boundary.
/// </summary>
public class AddMilestoneDependencyCommandValidator : AbstractValidator<AddMilestoneDependencyCommand>
{
    /// <summary>
    /// Initializes structural graph edge constraints for milestone dependency links.
    /// </summary>
    public AddMilestoneDependencyCommandValidator()
    {
        RuleFor(x => x.ProjectTemplateId)
            .NotEmpty()
            .WithMessage("The Parent Project Template Identifier context is required.");

        RuleFor(x => x.SuccessorId)
            .NotEmpty()
            .WithMessage("The dependent Successor Milestone Identifier is required.");

        RuleFor(x => x.PredecessorId)
            .NotEmpty()
            .WithMessage("The leading Predecessor Milestone Identifier is required.")
            .NotEqual(x => x.SuccessorId)
            .WithMessage("Application Layer Guard: A milestone node cannot establish a direct dependency constraint edge onto itself.");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("The requested structural dependency constraint rule type is invalid.");
    }
}