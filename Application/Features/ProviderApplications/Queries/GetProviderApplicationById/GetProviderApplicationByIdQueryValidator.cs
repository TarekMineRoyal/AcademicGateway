using FluentValidation;

namespace AcademicGateway.Application.Features.ProviderApplications.Queries.GetProviderApplicationById;

/// <summary>
/// Enforces structural validation constraints for incoming <see cref="GetProviderApplicationByIdQuery"/> requests.
/// </summary>
public class GetProviderApplicationByIdQueryValidator : AbstractValidator<GetProviderApplicationByIdQuery>
{
    /// <summary>
    /// Initializes validation rules for looking up provider application details by identifier.
    /// </summary>
    public GetProviderApplicationByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Provider application ID is required.");
    }
}