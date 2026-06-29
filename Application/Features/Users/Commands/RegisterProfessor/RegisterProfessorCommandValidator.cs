using AcademicGateway.Application.Common.Validations;
using FluentValidation;

namespace AcademicGateway.Application.Features.Users.Commands.RegisterProfessor;

public class RegisterProfessorCommandValidator : AbstractValidator<RegisterProfessorCommand>
{
    public RegisterProfessorCommandValidator()
    {
        // Base Identity Validations
        RuleFor(x => x.Email).ValidIdentityEmail();
        RuleFor(x => x.Username).ValidIdentityUsername();
        RuleFor(x => x.Password).ValidIdentityPassword();

        // Professor Specific Validations
        RuleFor(x => x.AcademicDepartment)
            .NotEmpty().WithMessage("Academic department is required.")
            .MaximumLength(100).WithMessage("Academic department cannot exceed 100 characters.");
    }
}