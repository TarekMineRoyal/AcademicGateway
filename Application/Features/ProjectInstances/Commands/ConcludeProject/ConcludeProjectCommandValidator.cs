using FluentValidation;

namespace AcademicGateway.Application.Features.ProjectInstances.Commands.ConcludeProject;

public class ConcludeProjectCommandValidator : AbstractValidator<ConcludeProjectCommand>
{
    public ConcludeProjectCommandValidator()
    {
        RuleFor(x => x.ProjectInstanceId)
            .NotEmpty()
            .WithMessage("Project Instance ID is required to conclude the workspace.");
    }
}