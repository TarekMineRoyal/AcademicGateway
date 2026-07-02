using FluentValidation;

namespace AcademicGateway.Application.Features.ProjectInstances.Commands.CancelProject;

public class CancelProjectCommandValidator : AbstractValidator<CancelProjectCommand>
{
    public CancelProjectCommandValidator()
    {
        RuleFor(x => x.ProjectInstanceId)
            .NotEmpty()
            .WithMessage("Project Instance ID is required to cancel the workspace.");

        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Reason))
            .WithMessage("The cancellation reason description text cannot exceed 500 characters.");
    }
}