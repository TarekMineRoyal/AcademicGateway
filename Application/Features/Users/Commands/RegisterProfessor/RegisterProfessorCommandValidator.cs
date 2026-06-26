using FluentValidation;

namespace AcademicGateway.Application.Features.Users.Commands.RegisterProfessor;

public class RegisterProfessorCommandValidator : AbstractValidator<RegisterProfessorCommand>
{
    public RegisterProfessorCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Username).NotEmpty().MinimumLength(3);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);

        RuleFor(x => x.AcademicDepartment).NotEmpty().WithMessage("Academic department is required.");
    }
}