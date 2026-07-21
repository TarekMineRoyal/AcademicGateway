using AcademicGateway.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.Professors.Commands.UpdateProfessorProfile;

/// <summary>
/// Handles the transaction routine to process an <see cref="UpdateProfessorProfileCommand"/>.
/// Resolves the professor aggregate via the secure session execution context, updates core faculty fields, 
/// modifies mentoring capacities, synchronizes research interest matrices, and flushes states cleanly.
/// </summary>
public class UpdateProfessorProfileCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<UpdateProfessorProfileCommand>
{
    /// <summary>
    /// Processes the professor profile update, executes domain mutations, synchronizes aggregate collections, and saves changes.
    /// </summary>
    /// <param name="request">The structural data bundle carrying the updated faculty details and research selections.</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be canceled.</param>
    /// <returns>A completed asynchronous execution task.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown if the security session is unauthenticated or the underlying profile does not exist.</exception>
    public async Task Handle(UpdateProfessorProfileCommand request, CancellationToken cancellationToken)
    {
        // 1. Enforce active security session validation early before querying data pools
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("Access Denied: Authentication is mandatory to update faculty metrics.");
        }

        Guid professorId = currentUserService.UserId.Value;

        // 2. Retrieve the encapsulated Professor aggregate root including all tracked child collections
        var professor = await context.Professors
            .Include(p => p.ResearchInterests)
            .FirstOrDefaultAsync(p => p.Id == professorId, cancellationToken);

        // 3. Prevent side-channel exposure or mapping vulnerabilities by using a uniform exception boundary
        if (professor == null)
        {
            throw new UnauthorizedAccessException("Access Denied: Authorized profile context was not found.");
        }

        // 4. Mutate core primitive attributes and mentoring capacity parameters via formal domain methods
        professor.UpdateFacultyDetails(request.FullName, request.Department, request.Rank);
        professor.UpdateSupervisionCapacity(request.MaxSupervisionCapacity);
        professor.UpdateAboutMe(request.AboutMe);

        // 5. Synchronize Research Interest Alignments (DDD Differential Synchronization Pattern)
        var targetInterests = request.ResearchInterestIds ?? Array.Empty<Guid>();
        var currentInterests = professor.ResearchInterests.Select(ri => ri.ResearchInterestId).ToList();

        foreach (var interestId in targetInterests.Except(currentInterests))
        {
            professor.AddResearchInterest(interestId);
        }

        foreach (var interestId in currentInterests.Except(targetInterests))
        {
            professor.RemoveResearchInterest(interestId);
        }

        // 6. Commit outstanding aggregate alterations atomically down to relational storage structures
        await context.SaveChangesAsync(cancellationToken);
    }
}