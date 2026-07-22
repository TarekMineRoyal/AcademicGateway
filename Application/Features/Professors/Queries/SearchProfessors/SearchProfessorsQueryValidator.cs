using FluentValidation;

namespace AcademicGateway.Application.Features.Professors.Queries.SearchProfessors;

/// <summary>
/// Enforces structural validation constraints for incoming <see cref="SearchProfessorsQuery"/> requests.
/// </summary>
public class SearchProfessorsQueryValidator : AbstractValidator<SearchProfessorsQuery>
{
    /// <summary>
    /// Initializes validation rules for searching professor profiles with pagination options.
    /// </summary>
    public SearchProfessorsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page number must be greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Page size must be greater than 0.");
    }
}