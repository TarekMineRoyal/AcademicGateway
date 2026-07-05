using Application.Features.ProjectInstances.Commands.AddMilestoneComment;
using FluentValidation;
using System;

namespace AcademicGateway.Application.Features.ProjectInstances.Commands.AddMilestoneComment;

/// <summary>
/// Enforces basic data structure validation and data sanitization for incoming <see cref="AddMilestoneCommentCommand"/> requests.
/// Serves as the initial validation check before passing execution parameters down to the local milestone discussion domain engine.
/// </summary>
public class AddMilestoneCommentCommandValidator : AbstractValidator<AddMilestoneCommentCommand>
{
    /// <summary>
    /// Initializes data layout validation rules for cross-role milestone conversation entries.
    /// </summary>
    public AddMilestoneCommentCommandValidator()
    {
        RuleFor(x => x.ProjectInstanceId)
            .NotEmpty()
            .WithMessage("The Parent Project Instance Workspace Identifier context is mandatory.");

        RuleFor(x => x.LocalMilestoneId)
            .NotEmpty()
            .WithMessage("The target Local Milestone Identifier code coordinate is mandatory.");

        RuleFor(x => x.AuthorId)
            .NotEmpty()
            .WithMessage("The author account identifier tracking token is mandatory.");

        RuleFor(x => x.AuthorIdentitySnapshot)
            .NotEmpty()
            .WithMessage("The author identity snapshot role descriptor copy cannot be empty.")
            .MaximumLength(200)
            .WithMessage("The author identity snapshot role descriptor must not exceed 200 characters.");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("The discussion comment message copy content cannot be empty.")
            .MaximumLength(2000)
            .WithMessage("Application Layer Guard: Discussion comment copy contents cannot exceed the maximum length boundary of 2000 characters.");
    }
}