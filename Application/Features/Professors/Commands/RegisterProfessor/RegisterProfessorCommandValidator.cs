using AcademicGateway.Application.Common.Validations;
using FluentValidation;

namespace AcademicGateway.Application.Features.Professors.Commands.RegisterProfessor;

/// <summary>
/// Validates incoming input arguments for the <see cref="RegisterProfessorCommand"/> before handler routing occurs.
/// </summary>
public class RegisterProfessorCommandValidator : AbstractValidator<RegisterProfessorCommand>
{
    /// <summary>
    /// Initializes structural and format capability constraints for institutional faculty member registration.
    /// </summary>
    public RegisterProfessorCommandValidator()
    {
        // Base Identity Extension Mappings
        RuleFor(x => x.Email).ValidIdentityEmail();
        RuleFor(x => x.Username).ValidIdentityUsername();
        RuleFor(x => x.Password).ValidIdentityPassword();

        // Professor Aggregation Property Guards
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Professor faculty identity full name cannot be empty or whitespace.")
            .MaximumLength(150).WithMessage("Full name description details cannot exceed 150 characters.");

        RuleFor(x => x.AcademicDepartment)
            .NotEmpty().WithMessage("Academic department assignment details cannot be empty or whitespace.")
            .MaximumLength(100).WithMessage("Academic department cannot exceed 100 characters.");

        RuleFor(x => x.Rank)
            .NotEmpty().WithMessage("Faculty positional rank status details cannot be empty or whitespace.")
            .MaximumLength(50).WithMessage("Faculty positional rank status description cannot exceed 50 characters.");

        RuleFor(x => x.MaxSupervisionCapacity)
            .GreaterThan(0).WithMessage("Initial maximum supervisor project capacity limit bounds must exceed zero.");
    }
}