using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProviderApplications.Queries.GetProviderApplicationById;

/// <summary>
/// Handles the execution of the <see cref="GetProviderApplicationByIdQuery"/> request.
/// Fetches complete provider application metadata, credentials, document links, and historical evaluation details securely.
/// </summary>
public class GetProviderApplicationByIdQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    IIdentityService identityService) // Injected IdentityService
    : IRequestHandler<GetProviderApplicationByIdQuery, ProviderApplicationDto?>
{
    public async Task<ProviderApplicationDto?> Handle(
        GetProviderApplicationByIdQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Enforce active authentication guard
        if (!currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Access Denied: Authentication is mandatory to query provider application details.");
        }

        // 2. Project application entity directly to lightweight read DTO
        var dto = await context.ProviderApplications
            .AsNoTracking()
            .Where(a => a.Id == request.Id)
            .Select(a => new ProviderApplicationDto
            {
                Id = a.Id,
                ProviderId = a.ProviderId,
                CompanyName = a.Provider != null ? a.Provider.CompanyName : "Unknown Corporate Entity",
                CompanyDetails = a.CompanyDetails,
                VerificationDocumentsUrl = a.VerificationDocumentsUrl,
                Status = a.Status,
                ReviewedById = a.ReviewedById,
                ReviewerName = a.ReviewedBy != null ? a.ReviewedBy.FullName : null,
                RejectionReason = a.RejectionReason,
                CreatedAt = a.CreatedAt,
                ReviewedAt = a.ReviewedAt,
                History = new ProviderApplicationHistoryDto
                {
                    SubmittedAt = a.CreatedAt,
                    EvaluatedAt = a.ReviewedAt,
                    EvaluatedById = a.ReviewedById,
                    EvaluatedByName = a.ReviewedBy != null ? a.ReviewedBy.FullName : null,
                    RejectionReason = a.RejectionReason
                }
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (dto == null)
        {
            return null;
        }

        // 3. Authorization guard: Allow designated Reviewers/Admins or the owner Provider
        bool isReviewer = currentUserService.IsInRole(Roles.Reviewer) || currentUserService.IsInRole(Roles.Admin);
        bool isOwner = dto.ProviderId == currentUserService.UserId;

        if (!isReviewer && !isOwner)
        {
            throw new UnauthorizedAccessException("Access Denied: You do not possess the required reviewer privileges to access application details.");
        }

        // 4. Fetch the applicant's email from Identity and populate ContactEmail
        var emailMap = await identityService.GetUserEmailsAsync([dto.ProviderId], cancellationToken);
        var contactEmail = emailMap.TryGetValue(dto.ProviderId, out var email) ? email : string.Empty;

        return dto with { ContactEmail = contactEmail };
    }
}