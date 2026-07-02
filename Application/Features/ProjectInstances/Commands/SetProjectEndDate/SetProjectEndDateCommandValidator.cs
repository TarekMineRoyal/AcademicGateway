using FluentValidation;
using AcademicGateway.Application.Common.Interfaces;

namespace AcademicGateway.Application.Features.ProjectInstances.Commands.SetProjectEndDate;

/// <summary>
/// Enforces consistency rules for incoming project deadline modifications using a deterministic clock.
/// </summary>
public class SetProjectEndDateCommandValidator : AbstractValidator<SetProjectEndDateCommand>
{
    /// <summary>
    /// Initializes validation constraints for the project end date boundary adjustment.
    /// </summary>
    /// <param name="dateTimeProvider">The injected deterministic system clock abstraction layer.</param>
    public SetProjectEndDateCommandValidator(IDateTimeProvider dateTimeProvider)
    {
        RuleFor(x => x.ProjectInstanceId)
            .NotEmpty()
            .WithMessage("Project Instance ID is required to update its calendar boundaries.");

        // Changing from '() =>' to 'x =>' fixes the CS0453 type constraint compilation error
        RuleFor(x => x.NewEndDate)
            .GreaterThan(x => dateTimeProvider.UtcNow)
            .WithMessage("The newly proposed project end date must be set to a future calendar date.");
    }
}