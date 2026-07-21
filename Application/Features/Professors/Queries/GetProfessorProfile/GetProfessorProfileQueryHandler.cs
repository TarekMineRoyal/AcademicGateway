using AcademicGateway.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.Professors.Queries.GetProfessorProfile;

/// <summary>
/// Handles the execution of the <see cref="GetProfessorProfileQuery"/> request.
/// Safely utilizes untracked relational projection to map rich domain metrics straight to presentation structures securely.
/// </summary>
public class GetProfessorProfileQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetProfessorProfileQuery, ProfessorProfileDto>
{
    /// <summary>
    /// Processes the query transaction securely by locating the professor aggregate root and verifying session tenancy.
    /// </summary>
    /// <param name="request">The structural parameter bundle identifying the requested professor record.</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be canceled.</param>
    /// <returns>A comprehensive view of the targeted professor profile information.</returns>
    /// <exception cref="UnauthorizedAccessException">Uniformly thrown if session authentication fails, the resource is missing, or tenancy validation fails.</exception>
    public async Task<ProfessorProfileDto> Handle(GetProfessorProfileQuery request, CancellationToken cancellationToken)
    {
        // Enforce active security session validation early before executing database logic
        if (!currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Access Denied: Authentication is mandatory to query academic profiles.");
        }

        // Verify tenancy alignment: Users can only query their own specific professor profile context
        if (request.ProfessorId != currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("Access Denied: The requested profile was not found, or you do not possess read authorization permissions.");
        }

        // Project the relational database tables directly into clean presentation contracts.
        var profile = await context.Professors
            .AsNoTracking()
            .Where(p => p.Id == request.ProfessorId)
            .Select(p => new ProfessorProfileDto
            {
                Id = p.Id,
                FullName = p.FullName,
                Department = p.Department,
                Rank = p.Rank,
                AboutMe = p.AboutMe,
                MaxSupervisionCapacity = p.MaxSupervisionCapacity,
                CurrentProjectCount = p.CurrentProjectCount,
                IsAcceptingProjects = p.CurrentProjectCount < p.MaxSupervisionCapacity
            })
            .FirstOrDefaultAsync(cancellationToken);

        // Validate presence boundaries uniformly to protect against resource scanning behaviors
        if (profile == null)
        {
            throw new UnauthorizedAccessException("Access Denied: The requested profile was not found, or you do not possess read authorization permissions.");
        }

        return profile;
    }
}