using FluentValidation;

namespace AcademicGateway.Application.Features.TechSupportProposals.Commands.ReviewTechSupportProposal;

/// <summary>
/// Enforces consistency and length boundaries for incoming <see cref="ReviewTechSupportProposalCommand"/> request models.
/// </summary>
public class ReviewTechSupportProposalCommandValidator : AbstractValidator<ReviewTechSupportProposalCommand>
{
    /// <summary>
    /// Initializes validation constraints for reviewing an industry support proposal.
    /// </summary>
    public ReviewTechSupportProposalCommandValidator()
    {
        RuleFor(x => x.ProjectInstanceId)
            .NotEmpty()
            .WithMessage("Project Instance ID is required to locate the target workspace board.");

        RuleFor(x => x.TechSupportProposalId)
            .NotEmpty()
            .WithMessage("Tech Support Proposal ID is required to identify the target matching row.");

        // Clean Check: Ensure a rejection reason is not supplied if the student is accepting the offer
        RuleFor(x => x.RejectionReason)
            .Empty()
            .When(x => x.Accept)
            .WithMessage("A rejection reason should not be specified when accepting a corporate mentor offer.");

        // Text Boundary Check: Restrict length if a comment is left while declining
        RuleFor(x => x.RejectionReason)
            .MaximumLength(500)
            .When(x => !x.Accept)
            .WithMessage("The feedback explanation text cannot exceed 500 characters.");
    }
}