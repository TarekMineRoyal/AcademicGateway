using FluentValidation;

namespace AcademicGateway.Application.Features.TechSupportProposals.Queries.GetTechSupportProposals;

/// <summary>
/// Enforces structural validation constraints for incoming <see cref="GetTechSupportProposalsQuery"/> requests.
/// </summary>
public class GetTechSupportProposalsQueryValidator : AbstractValidator<GetTechSupportProposalsQuery>
{
    /// <summary>
    /// Initializes validation rules for querying corporate assistance offers associated with a project workspace.
    /// </summary>
    public GetTechSupportProposalsQueryValidator()
    {
        RuleFor(x => x.ProjectInstanceId)
            .NotEmpty()
            .WithMessage("Project Instance ID is required to locate the active workspace aggregate.");

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page number must be greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Page size must be greater than 0.");
    }
}