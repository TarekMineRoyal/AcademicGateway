using MediatR;
using System;
using System.Collections.Generic;

namespace AcademicGateway.Application.Features.SupervisionRequests.Queries.GetPendingSupervisionRequests;

/// <summary>
/// CQRS query request contract targeting outstanding academic supervision invitations.
/// Requests a filtered collection of pending requests assigned to a specific faculty member.
/// </summary>
public class GetPendingSupervisionRequestsQuery : IRequest<IReadOnlyCollection<PendingSupervisionRequestDto>>
{
    /// <summary>
    /// Gets or sets the target faculty member's unique lookup identifier key.
    /// </summary>
    public Guid ProfessorId { get; set; }
}