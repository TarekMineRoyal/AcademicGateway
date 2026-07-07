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
    }
}