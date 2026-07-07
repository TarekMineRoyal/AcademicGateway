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
/// Fortified against Broken Object Level Authorization (BOLA) and side-channel resource enumeration vectors.
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
    /// Validates workspace ownership, invokes the aggregate track pivot state machine, and flushes database changes securely.
    /// </summary>
    /// <param name="request">The incoming CQRS data payload containing the target project instance tracking ID.</param>
    /// <param name="cancellationToken">The operational signal tracking asynchronous execution cancellations.</param>
    /// <returns>A transactional execution confirmation wrapper unit.</returns>
    /// <exception cref="UnauthorizedAccessException">Uniformly thrown if an unauthenticated user or separate entity attempts to access the workspace.</exception>
    public async Task<Unit> Handle(TransitionToSoloCommand request, CancellationToken cancellationToken)
    {
        // 1. Enforce active session validation before performing downstream persistence layer operations
        if (!_currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Access Denied: Authentication is mandatory to transition workspace tracks.");
        }

        // 2. Retrieve the target project instance along with its current supervision requests to allow internal cleanups
        var projectInstance = await _context.ProjectInstances
            .Include(pi => pi.SupervisionRequests)
            .FirstOrDefaultAsync(pi => pi.Id == request.ProjectInstanceId, cancellationToken);

        // 3. Protect against side-channel resource enumeration
        // Uniformly respond with an UnauthorizedAccessException if the item does not exist or if it belongs to another user.
        // This ensures external scanning mechanisms cannot determine resource presence.
        if (projectInstance == null || projectInstance.StudentId != _currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("Access Denied: The requested project workspace was not found, or you do not possess ownership authorization permissions.");
        }

        // 4. Delegate core state machine mutations downstream to the Aggregate Root
        // This clears pending requests, updates internal tracking parameters, and fires corresponding events.
        projectInstance.TransitionToSolo();

        // 5. Commit state modifications down to physical relational rows atomically
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}