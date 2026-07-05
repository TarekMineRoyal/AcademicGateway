using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGateway.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGateway.Application.Features.SupervisionRequests.Commands.ReviewSupervisionRequest;

/// <summary>
/// Orchestrates the application pipeline for an academic supervisor to accept or decline an outstanding matchmaking invitation.
/// </summary>
public class ReviewSupervisionRequestCommandHandler : IRequestHandler<ReviewSupervisionRequestCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReviewSupervisionRequestCommandHandler"/> class.
    /// </summary>
    /// <param name="context">The unit-of-work relational data access boundary layer.</param>
    /// <param name="currentUserService">Provides tracking visibility over the currently authenticated session security credentials.</param>
    /// <param name="dateTimeProvider">The deterministic system clock wrapper abstraction.</param>
    public ReviewSupervisionRequestCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    /// <summary>
    /// Validates professor ownership permissions, invokes the aggregate review state machine, and saves changes.
    /// </summary>
    /// <param name="request">The incoming CQRS data payload containing evaluation routing keys and decisions.</param>
    /// <param name="cancellationToken">The operational signal tracking asynchronous execution cancellations.</param>
    /// <returns>A transactional execution confirmation wrapper unit.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if either the target project instance or specific invitation log does not exist.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if an unauthenticated user or separate professor attempts to review the invitation.</exception>
    public async Task<Unit> Handle(ReviewSupervisionRequestCommand request, CancellationToken cancellationToken)
    {
        // 1. Retrieve the target project instance along with its current supervision request entity log records
        var projectInstance = await _context.ProjectInstances
            .Include(pi => pi.SupervisionRequests)
            .FirstOrDefaultAsync(pi => pi.Id == request.ProjectInstanceId, cancellationToken);

        if (projectInstance == null)
        {
            throw new KeyNotFoundException($"The target project instance workspace with ID '{request.ProjectInstanceId}' was not found.");
        }

        // 2. Locate the individual invitation child entity mapping within our aggregate collection boundary
        var supervisionRequest = projectInstance.SupervisionRequests
            .FirstOrDefault(r => r.Id == request.SupervisionRequestId);

        if (supervisionRequest == null)
        {
            throw new KeyNotFoundException($"The requested academic supervision invite record with ID '{request.SupervisionRequestId}' was not found within this project context.");
        }

        // 3. Security Boundaries: Enforce that only the specific targeted professor can execute this evaluation review[cite: 2]
        if (!_currentUserService.IsAuthenticated || supervisionRequest.ProfessorId != _currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("Access Denied: You are not authorized to evaluate this supervision invitation.");
        }

        // 4. Delegate core state transition checks and multi-track state evaluations downstream to the Aggregate Root
        projectInstance.ReviewSupervisionRequest(
            request.SupervisionRequestId,
            request.Accept,
            request.RejectionReason,
            _dateTimeProvider.UtcNow);

        // 5. Commit state modifications down to physical relational rows atomically
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}