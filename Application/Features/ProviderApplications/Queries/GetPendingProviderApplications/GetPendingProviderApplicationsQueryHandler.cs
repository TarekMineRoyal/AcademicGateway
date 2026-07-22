using AcademicGateway.Application.Common.Extensions;
using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Common.Models;
using AcademicGateway.Domain.Common.Constants;
using AcademicGateway.Domain.Providers.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProviderApplications.Queries.GetPendingProviderApplications;

public class GetPendingProviderApplicationsQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    IIdentityService identityService)
    : IRequestHandler<GetPendingProviderApplicationsQuery, PaginatedResult<PendingProviderApplicationDto>>
{
    public async Task<PaginatedResult<PendingProviderApplicationDto>> Handle(
        GetPendingProviderApplicationsQuery request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Access Denied: Authentication is mandatory to query administrative operational queues.");
        }

        if (!currentUserService.IsInRole(Roles.Reviewer))
        {
            throw new UnauthorizedAccessException("Access Denied: You do not possess the required administrative privileges to view the registration queue.");
        }

        // 1. Fetch paginated pending application records
        var paginatedApps = await context.ProviderApplications
            .AsNoTracking()
            .Where(a => a.Status == ProviderApplicationStatus.PendingReview)
            .OrderBy(a => a.CreatedAt)
            .Select(a => new
            {
                a.Id,
                a.ProviderId,
                ProviderName = a.Provider != null ? a.Provider.CompanyName : "Unknown Corporate Entity",
                CredentialsSummary = a.CompanyDetails,
                SubmittedAt = a.CreatedAt
            })
            .ToPaginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);

        // 2. Fetch corresponding emails via IdentityService
        var providerIds = paginatedApps.Items.Select(x => x.ProviderId).Distinct();
        var emailMap = await identityService.GetUserEmailsAsync(providerIds, cancellationToken);

        // 3. Map into presentation DTOs
        var dtos = paginatedApps.Items.Select(a => new PendingProviderApplicationDto
        {
            Id = a.Id,
            ProviderName = a.ProviderName,
            CredentialsSummary = a.CredentialsSummary,
            SubmittedAt = a.SubmittedAt,
            ContactEmail = emailMap.TryGetValue(a.ProviderId, out var email) ? email : string.Empty
        }).ToList();

        return new PaginatedResult<PendingProviderApplicationDto>(
            dtos,
            paginatedApps.TotalCount,
            paginatedApps.PageNumber,
            paginatedApps.PageSize);
    }
}