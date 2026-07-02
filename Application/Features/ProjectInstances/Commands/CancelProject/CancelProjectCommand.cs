using MediatR;
using System;

namespace AcademicGateway.Application.Features.ProjectInstances.Commands.CancelProject;

public record CancelProjectCommand : IRequest<Unit>
{
    public Guid ProjectInstanceId { get; init; }
    public string? Reason { get; init; }
}