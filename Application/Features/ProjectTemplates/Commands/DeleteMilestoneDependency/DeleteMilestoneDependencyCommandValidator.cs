using FluentValidation;
using System;

namespace AcademicGateway.Application.Features.ProjectTemplates.Commands.DeleteMilestoneDependency;

/// <summary>
/// Proactively validates incoming graph structural inputs for the <see cref="DeleteMilestoneDependencyCommand"/> 
/// before dispatching the request to the application execution pipeline handler.
/// </summary>
public class DeleteMilestoneDependencyCommandValidator : AbstractValidator<DeleteMilestoneDependencyCommand>
{
    /// <summary>
    /// Initializes structural graph edge validation constraints for removing milestone dependency links.
    /// </summary>
    public DeleteMilestoneDependencyCommandValidator()
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
            .WithMessage("Application Layer Guard: A milestone node cannot look for or sever a dependency constraint edge mapped onto itself.");
    }
}