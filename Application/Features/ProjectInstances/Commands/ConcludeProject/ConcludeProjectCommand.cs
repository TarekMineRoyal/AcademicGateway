using MediatR;
using System;

namespace AcademicGateway.Application.Features.ProjectInstances.Commands.ConcludeProject;

public record ConcludeProjectCommand : IRequest<Unit>
{
    public Guid ProjectInstanceId { get; init; }
}