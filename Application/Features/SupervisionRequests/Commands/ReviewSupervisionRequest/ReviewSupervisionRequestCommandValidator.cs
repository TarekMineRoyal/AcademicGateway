using FluentValidation;

namespace AcademicGateway.Application.Features.SupervisionRequests.Commands.ReviewSupervisionRequest;

/// <summary>
/// Enforces structural and contextual validation constraints for incoming <see cref="ReviewSupervisionRequestCommand"/> requests.
/// </summary>
public class ReviewSupervisionRequestCommandValidator : AbstractValidator<ReviewSupervisionRequestCommand>
{
    /// <summary>
    /// Initializes validation rules for evaluation reviews submitted by academic supervisors.
    /// </summary>
    public ReviewSupervisionRequestCommandValidator()
    {
        RuleFor(x => x.ProjectInstanceId)
            .NotEmpty()
            .WithMessage("Project Instance ID is required to locate the active workspace.");

        RuleFor(x => x.SupervisionRequestId)
            .NotEmpty()
            .WithMessage("Supervision Request ID is required to update the matchmaking records.");

        // Strict Guard: If the professor accepts, enforce that the rejection reason is completely null or empty
        RuleFor(x => x.RejectionReason)
            .Empty()
            .When(x => x.Accept)
            .WithMessage("A rejection reason must not be provided when accepting an academic supervision request.");

        // Safe Boundary Bound: If a rejection reason is typed out during a refusal, restrict its structural capacity bounds
        RuleFor(x => x.RejectionReason)
            .MaximumLength(500)
            .When(x => !x.Accept)
            .WithMessage("Rejection reason feedback text cannot exceed 500 characters.");
    }
}