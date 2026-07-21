using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AcademicGateway.Application.Features.Professors.Commands.UpdateProfessorProfile;

/// <summary>
/// Validates incoming arguments for the <see cref="UpdateProfessorProfileCommand"/> before handler routing occurs.
/// Enforces domain-aligned text boundaries, structural lengths, and capacity ceiling rules.
/// </summary>
public class UpdateProfessorProfileCommandValidator : AbstractValidator<UpdateProfessorProfileCommand>
{
    /// <summary>
    /// Initializes functional format constraints and bounds filters for professor profile mutations.
    /// </summary>
    public UpdateProfessorProfileCommandValidator()
    {
        // Enforce Legal Faculty Identity Profile Boundaries
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Professor faculty identity full name cannot be empty or whitespace.")
            .MaximumLength(150).WithMessage("Full name description details cannot exceed 150 characters.");

        RuleFor(x => x.Department)
            .NotEmpty().WithMessage("Academic department assignment details cannot be empty or whitespace.")
            .MaximumLength(100).WithMessage("Academic department cannot exceed 100 characters.");

        RuleFor(x => x.Rank)
            .NotEmpty().WithMessage("Faculty positional rank status details cannot be empty or whitespace.")
            .MaximumLength(50).WithMessage("Faculty positional rank status description cannot exceed 50 characters.");

        RuleFor(x => x.AboutMe)
            .MaximumLength(2000).WithMessage("About me biography text cannot exceed 2000 characters.");

        // Capacity Ceiling Boundary Rules
        RuleFor(x => x.MaxSupervisionCapacity)
            .GreaterThan(0).WithMessage("Altered maximum supervisor project capacity limit bounds must exceed zero.");

        // Structural Collection Validations
        RuleFor(x => x.ResearchInterestIds)
            .Must(list => list == null || list.Count <= 15).WithMessage("You cannot assign more than 15 active research interest alignments to your profile.");
    }
}