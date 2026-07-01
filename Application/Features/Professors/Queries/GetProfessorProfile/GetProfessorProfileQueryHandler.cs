using AcademicGateway.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.Professors.Queries.GetProfessorProfile;

/// <summary>
/// Handles the execution of the <see cref="GetProfessorProfileQuery"/> request.
/// Safely utilizes untracked relational projection to map rich domain metrics straight to presentation structures.
/// </summary>
public class GetProfessorProfileQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetProfessorProfileQuery, ProfessorProfileDto>
{
    /// <summary>
    /// Processes the query transaction by locating the professor aggregate root and projecting a read-only snapshot.
    /// </summary>
    /// <param name="request">The structural parameter bundle identifying the requested professor record.</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be canceled.</param>
    /// <returns>A comprehensive view of the targeted professor profile information.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if no matching professor aggregate profile is found in the database.</exception>
    public async Task<ProfessorProfileDto> Handle(GetProfessorProfileQuery request, CancellationToken cancellationToken)
    {
        var profile = await context.Professors
            .AsNoTracking()
            .Where(p => p.Id == request.ProfessorId) // Aligned with the Guid domain key transformation
            .Select(p => new ProfessorProfileDto
            {
                Id = p.Id,
                FullName = p.FullName,
                AcademicDepartment = p.Department, // Maps domain entity 'Department' to DTO 'AcademicDepartment'
                Rank = p.Rank,
                MaxSupervisionCapacity = p.MaxSupervisionCapacity,
                CurrentProjectCount = p.CurrentProjectCount,
                IsAcceptingProjects = p.IsAcceptingProjects
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (profile == null)
        {
            throw new KeyNotFoundException($"Professor profile for ID '{request.ProfessorId}' was not found.");
        }

        return profile;
    }
}