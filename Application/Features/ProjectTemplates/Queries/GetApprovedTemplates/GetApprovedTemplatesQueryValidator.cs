using FluentValidation;

namespace AcademicGateway.Application.Features.ProjectTemplates.Queries.GetApprovedTemplates;

/// <summary>
/// Enforces structural validation constraints for incoming <see cref="GetApprovedTemplatesQuery"/> requests.
/// </summary>
public class GetApprovedTemplatesQueryValidator : AbstractValidator<GetApprovedTemplatesQuery>
{
    /// <summary>
    /// Initializes validation rules for retrieving approved project template blueprints.
    /// </summary>
    public GetApprovedTemplatesQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page number must be greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Page size must be greater than 0.");
    }
}