using AcademicGateway.Application.Common.Models;
using MediatR;
using System;

namespace AcademicGateway.Application.Features.TechSupportProposals.Queries.GetTechSupportProposals;

/// <summary>
/// CQRS query request contract targeting corporate industry mentorship proposals.
/// Requests the active assistance offers tied to a specific project workspace instance.
/// </summary>
public record GetTechSupportProposalsQuery(
    Guid ProjectInstanceId,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PaginatedResult<TechSupportProposalDto>>;