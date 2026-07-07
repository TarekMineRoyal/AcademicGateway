using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.ProjectTemplates.Enums;
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
        if (!currentUserService.IsInRole("Reviewer"))
        {
            throw new UnauthorizedAccessException("Access Denied: You do not possess the required administrative privileges to view the blueprint clearance queue.");
        }

        // 3. Project database records directly into the lightweight read contract.
        // Performance Optimization: Direct LINQ projections instruct EF Core to generate precise SQL statements,
        // bypassing change-tracking buffers completely and avoiding over-fetching heavy nested collections.
        return await context.ProjectTemplates
            .AsNoTracking()
            // Fixed: Updated to match 'PendingReview' to correctly reference the domain enum structural definition
            .Where(t => t.Status == ProjectTemplateStatus.PendingReview)
            .OrderBy(t => t.CreatedAt) // Oldest templates prioritized first in processing cycles (FIFO queue)
            .Select(t => new PendingProjectTemplateDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                // Gracefully navigate the navigation layout anchor to expose provider ownership details safely
                ProviderName = t.Provider != null ? t.Provider.CompanyName : "Independent Industry Author",
                SubmittedAt = t.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}