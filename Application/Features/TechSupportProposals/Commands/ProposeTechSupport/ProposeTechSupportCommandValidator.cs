using FluentValidation;

namespace AcademicGateway.Application.Features.TechSupportProposals.Commands.ProposeTechSupport;

/// <summary>
/// Enforces business validation rules for incoming <see cref="ProposeTechSupportCommand"/> requests.
/// </summary>
public class ProposeTechSupportCommandValidator : AbstractValidator<ProposeTechSupportCommand>
{
    /// <summary>
    /// Initializes validation constraints for proposing a corporate mentor attachment.
    /// </summary>
    public ProposeTechSupportCommandValidator()
    {
        RuleFor(x => x.ProjectInstanceId)
            .NotEmpty()
            .WithMessage("Project Instance ID is required to route the technical support proposal.");

        RuleFor(x => x.TechSupportAccountId)
            .NotEmpty()
            .WithMessage("Tech Support Account ID is required to identify the proposed engineer.");

        // Safe Boundary Guard: If an optional message is provided, restrict its structural size bounds
        RuleFor(x => x.Message)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Message))
            .WithMessage("The introductory message text cannot exceed 1000 characters.");
    }
}