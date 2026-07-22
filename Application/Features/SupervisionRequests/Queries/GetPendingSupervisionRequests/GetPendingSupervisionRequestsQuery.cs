using AcademicGateway.Application.Common.Models;
using MediatR;
using System;

namespace AcademicGateway.Application.Features.SupervisionRequests.Queries.GetPendingSupervisionRequests;

/// <summary>
/// CQRS query request contract targeting outstanding academic supervision invitations.
/// Requests a filtered collection of pending requests assigned to a specific faculty member.
/// </summary>
/// <param name="ProfessorId">The target faculty member's unique lookup identifier key.</param>
/// <param name="PageNumber">The 1-based index of the page to retrieve (default: 1).</param>
/// <param name="PageSize">The maximum number of items to retrieve per page (default: 10).</param>
public record GetPendingSupervisionRequestsQuery(
    Guid ProfessorId,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PaginatedResult<PendingSupervisionRequestDto>>;