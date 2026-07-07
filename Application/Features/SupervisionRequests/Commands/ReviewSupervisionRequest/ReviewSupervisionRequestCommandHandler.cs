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
    /// Validates professor ownership permissions, invokes the aggregate review state machine, and saves changes securely.
    /// </summary>
    /// <param name="request">The incoming CQRS data payload containing evaluation routing keys and decisions.</param>
    /// <param name="cancellationToken">The operational signal tracking asynchronous execution cancellations.</param>
    /// <returns>A transactional execution confirmation wrapper unit.</returns>
    /// <exception cref="UnauthorizedAccessException">Uniformly thrown if the session is unauthenticated, the resource is missing, or tenancy fails validation.</exception>
    public async Task<Unit> Handle(ReviewSupervisionRequestCommand request, CancellationToken cancellationToken)
    {
        // 1. Enforce active security session validation early before executing relational queries
        if (!_currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Access Denied: Authentication is mandatory to evaluate academic supervision invitations.");
        }

        // 2. Retrieve the target project instance along with its tracking supervision request entity log records
        var projectInstance = await _context.ProjectInstances
            .Include(pi => pi.SupervisionRequests)
            .FirstOrDefaultAsync(pi => pi.Id == request.ProjectInstanceId, cancellationToken);

        // 3. Extract the targeted individual invitation child entity mapping configuration if it exists
        var supervisionRequest = projectInstance?.SupervisionRequests
            .FirstOrDefault(r => r.Id == request.SupervisionRequestId);

        // 4. Protect against side-channel resource enumeration
        // Coalesce parent aggregate null checks, child entity null checks, and targeted ownership verification checks.
        // Uniformly throw an UnauthorizedAccessException to prevent metadata leakage concerning resource presence.
        if (projectInstance == null || supervisionRequest == null || supervisionRequest.ProfessorId != _currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("Access Denied: The requested invitation record was not found, or you do not possess evaluation authorization permissions.");
        }

        // 5. Delegate core state transition checks and multi-track state evaluations downstream to the Aggregate Root boundary
        projectInstance.ReviewSupervisionRequest(
            request.SupervisionRequestId,
            request.Accept,
            request.RejectionReason,
            _dateTimeProvider.UtcNow);

        // 6. Commit state modifications down to physical relational rows atomically
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}