using FluentValidation;

namespace AcademicGateway.Application.Features.SupervisionRequests.Queries.GetPendingSupervisionRequests;

/// <summary>
/// Enforces structural validation constraints for incoming <see cref="GetPendingSupervisionRequestsQuery"/> requests.
/// </summary>
public class GetPendingSupervisionRequestsQueryValidator : AbstractValidator<GetPendingSupervisionRequestsQuery>
{
    /// <summary>
    /// Initializes validation rules for filtering pending supervision invitations assigned to an academic supervisor.
    /// </summary>
    public GetPendingSupervisionRequestsQueryValidator()
    {
        RuleFor(x => x.ProfessorId)
            .NotEmpty()
            .WithMessage("Professor ID is required to look up incoming matchmaking invitations.");

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page number must be greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Page size must be greater than 0.");
    }
}