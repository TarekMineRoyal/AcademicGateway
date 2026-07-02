using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AcademicGateway.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGateway.Application.Features.ProjectInstances.Commands.TransitionToSolo;

/// <summary>
/// Orchestrates the application pipeline to pivot a project workspace from an onboarding matchmaking freeze into a solo execution track.
/// </summary>
public class TransitionToSoloCommandHandler : IRequestHandler<TransitionToSoloCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransitionToSoloCommandHandler"/> class.
    /// </summary>
    /// <param name="context">The unit-of-work relational data access boundary layer.</param>
    /// <param name="currentUserService">Provides tracking visibility over the currently authenticated session security credentials.</param>
    public TransitionToSoloCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Validates workspace ownership, invokes the aggregate track pivot state machine, and flushes database changes.
    /// </summary>
    /// <param name="request">The incoming CQRS data payload containing the target project instance tracking ID.</param>
    /// <param name="cancellationToken">The operational signal tracking asynchronous execution cancellations.</param>
    /// <returns>A transactional execution confirmation wrapper unit.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the target project instance workspace cannot be located.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if an unauthenticated user or separate student attempts to override the tracking track.</exception>
    public async Task<Unit> Handle(TransitionToSoloCommand request, CancellationToken cancellationToken)
    {
        // 1. Retrieve the target project instance along with its current supervision requests to allow internal cleanups
        var projectInstance = await _context.ProjectInstances
            .Include(pi => pi.SupervisionRequests)
            .FirstOrDefaultAsync(pi => pi.Id == request.ProjectInstanceId, cancellationToken);

        if (projectInstance == null)
        {
            throw new KeyNotFoundException($"The target project instance workspace with ID '{request.ProjectInstanceId}' was not found.");
        }

        // 2. Security Boundaries: Enforce that only the student owner of this workspace can trigger a track override
        if (!_currentUserService.IsAuthenticated || projectInstance.StudentId != _currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("Access Denied: You are not authorized to alter the execution track of this project workspace.");
        }

        // 3. Delegate core state machine mutations downstream to the Aggregate Root
        // This clears pending requests, updates internal tracking parameters, and fires corresponding events.
        projectInstance.TransitionToSolo();

        // 4. Commit state modifications down to physical relational rows atomically
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}