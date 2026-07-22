using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGateway.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGateway.Application.Features.Professors.Queries.GetPublicProfessorProfile;

/// <summary>
/// Handles the execution of the <see cref="GetPublicProfessorProfileQuery"/> request.
/// Retrieves an individual professor's public profile details without user tenancy restrictions.
/// </summary>
public class GetPublicProfessorProfileQueryHandler(
    IApplicationDbContext context,
    IIdentityService identityService)
    : IRequestHandler<GetPublicProfessorProfileQuery, GetPublicProfessorProfileQueryDto?>
{
    /// <summary>
    /// Processes the public profile lookup request by querying the target professor aggregate root and attaching user identity metrics.
    /// </summary>
    /// <param name="request">The query request holding the targeted professor identification key.</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be canceled.</param>
    /// <returns>A mapped public profile DTO if found; otherwise, null.</returns>
    public async Task<GetPublicProfessorProfileQueryDto?> Handle(
        GetPublicProfessorProfileQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Resolve matching professor user identity details for email resolution
        var identityProfessors = await identityService.SearchProfessorsAsync(null, cancellationToken);
        var matchingIdentity = identityProfessors.FirstOrDefault(x => x.Id == request.ProfessorId);

        // 2. Query and project relational database tables into clean public presentation contracts
        var profile = await context.Professors
            .AsNoTracking()
            .Where(p => p.Id == request.ProfessorId)
            .Select(p => new GetPublicProfessorProfileQueryDto
            {
                Id = p.Id,
                FullName = p.FullName,
                Email = matchingIdentity != null ? matchingIdentity.Email : string.Empty,
                Department = p.Department,
                Rank = p.Rank,
                AboutMe = p.AboutMe,
                ResearchInterests = p.ResearchInterests
                    .Select(ri => ri.ResearchInterest != null ? ri.ResearchInterest.Area : string.Empty)
                    .Where(area => !string.IsNullOrWhiteSpace(area))
                    .ToList(),
                MaxSupervisionCapacity = p.MaxSupervisionCapacity,
                CurrentProjectCount = p.CurrentProjectCount,
                IsAcceptingProjects = p.CurrentProjectCount < p.MaxSupervisionCapacity
            })
            .FirstOrDefaultAsync(cancellationToken);

        return profile;
    }
}