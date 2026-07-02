using MediatR;
using System;

namespace AcademicGateway.Application.Features.SupervisionRequests.Commands.ReviewSupervisionRequest;

/// <summary>
/// CQRS Command for an academic professor to approve or decline a student's supervision invitation.
/// </summary>
public record ReviewSupervisionRequestCommand : IRequest<Unit>
{
    /// <summary>
    /// Gets the unique tracking identifier of the parent project instance workspace.
    /// </summary>
    public Guid ProjectInstanceId { get; init; }

    /// <summary>
    /// Gets the unique tracking identifier of the supervision request being evaluated.
    /// </summary>
    public Guid SupervisionRequestId { get; init; }

    /// <summary>
    /// Gets a value indicating whether the professor accepts or declines the project supervision request.
    /// True indicates acceptance; False indicates rejection.
    /// </summary>
    public bool Accept { get; init; }

    /// <summary>
    /// Gets the optional explanatory notes or feedback text detailing why an invitation was declined.
    /// This field is mandatory when <see cref="Accept"/> is false.
    /// </summary>
    public string? RejectionReason { get; init; }
}