using FluentValidation;
using System;

namespace AcademicGateway.Application.Features.ProjectInstances.Commands.FinalizeProject;

/// <summary>
/// Enforces basic data structure validation and data sanitization for incoming <see cref="FinalizeProjectCommand"/> requests.
/// Serves as the initial validation check before passing execution parameters to the domain finalization engine.
/// </summary>
public class FinalizeProjectCommandValidator : AbstractValidator<FinalizeProjectCommand>
{
    /// <summary>
    /// Initializes data layout validation rules for macro project grade finalization requests.
    /// </summary>
    public FinalizeProjectCommandValidator()
    {
        RuleFor(x => x.ProjectInstanceId)
            .NotEmpty()
            .WithMessage("The Target Project Instance Workspace Identifier context is mandatory.");

        RuleFor(x => x.ExecutingUserId)
            .NotEmpty()
            .WithMessage("The executing operator tracking identifier is mandatory to verify authorization permissions.");
    }
}