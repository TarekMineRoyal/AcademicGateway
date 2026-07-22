using AcademicGateway.Application.Common.Models;
using MediatR;

namespace AcademicGateway.Application.Features.ProviderApplications.Queries.GetPendingProviderApplications;

/// <summary>
/// CQRS query for retrieving the operational queue of provider onboarding applications currently awaiting evaluation.
/// Consumed exclusively by authorized administrative quality assurance reviewers to manage verification pipelines.
/// </summary>
/// <param name="PageNumber">The 1-based index of the page to retrieve (default: 1).</param>
/// <param name="PageSize">The maximum number of items to retrieve per page (default: 10).</param>
public record GetPendingProviderApplicationsQuery(
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PaginatedResult<PendingProviderApplicationDto>>;