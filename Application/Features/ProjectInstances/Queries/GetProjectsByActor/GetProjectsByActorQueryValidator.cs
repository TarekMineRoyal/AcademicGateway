using FluentValidation;
using AcademicGateway.Domain.Common.Constants;

namespace AcademicGateway.Application.Features.ProjectInstances.Queries.GetProjectsByActor;

/// <summary>
/// Enforces structural validation constraints for incoming <see cref="GetProjectsByActorQuery"/> requests.
/// </summary>
public class GetProjectsByActorQueryValidator : AbstractValidator<GetProjectsByActorQuery>
{
    /// <summary>
    /// Initializes validation rules for multi-role dashboard queries based on actor profiles.
    /// </summary>
    public GetProjectsByActorQueryValidator()
    {
        RuleFor(x => x.ActorId)
            .NotEmpty()
            .WithMessage("Actor ID is required to look up assigned project workspaces.");

        RuleFor(x => x.Role)
            .NotEmpty()
            .WithMessage("Ecosystem user role context is required to determine the active dashboard routing data path.")
            .Must(role => role == Roles.Student || role == Roles.Professor || role == Roles.Provider)
            .WithMessage("The provided role must be a recognized ecosystem archetype (Student, Professor, or Provider).");
    }
}