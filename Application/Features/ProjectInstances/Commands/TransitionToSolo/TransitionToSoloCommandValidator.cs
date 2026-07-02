using FluentValidation;

namespace AcademicGateway.Application.Features.ProjectInstances.Commands.TransitionToSolo;

/// <summary>
/// Enforces basic argument consistency checks for incoming <see cref="TransitionToSoloCommand"/> payloads.
/// </summary>
public class TransitionToSoloCommandValidator : AbstractValidator<TransitionToSoloCommand>
{
    /// <summary>
    /// Initializes validation constraints for shifting a workspace to a solo execution track.
    /// </summary>
    public TransitionToSoloCommandValidator()
    {
        RuleFor(x => x.ProjectInstanceId)
            .NotEmpty()
            .WithMessage("Project Instance ID is required to identify the target workspace.");
    }
}