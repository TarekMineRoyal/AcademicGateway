using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.ProjectTemplates.Enums;
using AcademicGateway.Domain.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProjectTemplates.Queries.GetPendingProjectTemplates;

/// <summary>
/// Handles the execution of the <see cref="GetPendingProjectTemplatesQuery"/> request.
/// Compiles the operational blueprint clearance queue of pending industry project templates for authorized reviewers securely.
/// </summary>
public class GetPendingProjectTemplatesQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetPendingProjectTemplatesQuery, IReadOnlyCollection<PendingProjectTemplateDto>>
{
    /// <summary>
    /// Processes the template clearance queue query by selecting submitted templates straight into lightweight presentation DTO records securely.
    /// </summary>
    /// <param name="request">The incoming CQRS read model request container.</param>
    /// <param name="cancellationToken">Propagates notification that database network execution routines should be canceled.</param>
    /// <returns>A read-only collection containing immutable DTO representations of submitted project templates awaiting clearance.</returns>
    /// <exception cref="UnauthorizedAccessException">Uniformly thrown if session validation or explicit role checking parameters fail boundaries.</exception>
    public async Task<IReadOnlyCollection<PendingProjectTemplateDto>> Handle(
        GetPendingProjectTemplatesQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Enforce active security session validation early before executing infrastructure or database paths
        if (!currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Access Denied: Authentication is mandatory to query administrative operational queues.");
        }

        // 2. Fast-Path Authorization Guard: Leverage the optimized in-memory claim verification.
        // Ensures O(1) complexity performance by utilizing the user's active token context instead of hitting the database layer.
        if (!currentUserService.IsInRole(Roles.Reviewer))
        {
            throw new UnauthorizedAccessException("Access Denied: You do not possess the required administrative privileges to view the blueprint clearance queue.");
        }

        // 3. Project database records directly into the lightweight read contract using left outer joins
        // for Major and Specialty to avoid missing navigation property errors (CS1061).
        return await (
            from t in context.ProjectTemplates.AsNoTracking()
            where t.Status == ProjectTemplateStatus.PendingReview
            join m in context.Majors on t.MajorId equals m.Id into majors
            from m in majors.DefaultIfEmpty()
            join s in context.Specialties on t.SpecialtyId equals s.Id into specialties
            from s in specialties.DefaultIfEmpty()
            orderby t.CreatedAt // Oldest templates prioritized first in processing cycles (FIFO queue)
            select new PendingProjectTemplateDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                // Gracefully navigate the navigation layout anchor to expose provider ownership details safely
                ProviderName = t.Provider != null ? t.Provider.CompanyName : "Independent Industry Author",
                SubmittedAt = t.CreatedAt,
                MajorId = t.MajorId,
                SpecialtyId = t.SpecialtyId,
                MajorName = m != null ? m.Name : null,
                SpecialtyName = s != null ? s.Name : null
            }
        ).ToListAsync(cancellationToken);
    }
}