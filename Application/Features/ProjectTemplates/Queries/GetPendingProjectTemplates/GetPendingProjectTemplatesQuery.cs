using AcademicGateway.Application.Common.Models;
using MediatR;

namespace AcademicGateway.Application.Features.ProjectTemplates.Queries.GetPendingProjectTemplates;

/// <summary>
/// CQRS query for retrieving the operational blueprint clearance queue of project templates currently awaiting review.
/// Consumed exclusively by authorized administrative quality assurance reviewers to inspect, comment on, and approve incoming industry templates.
/// </summary>
/// <param name="PageNumber">The 1-based index of the page to retrieve (default: 1).</param>
/// <param name="PageSize">The maximum number of items to retrieve per page (default: 10).</param>
public record GetPendingProjectTemplatesQuery(
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PaginatedResult<PendingProjectTemplateDto>>;