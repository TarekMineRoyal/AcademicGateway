using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.ProjectInstances.Enums;
using AcademicGateway.Domain.ProjectInstances.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGateway.Application.Features.SupervisionRequests.Commands.SubmitSupervisionRequest;

/// <summary>
/// Orchestrates the application pipeline for a student to request academic supervision on an active project instance workspace.
/// </summary>
public class SubmitSupervisionRequestCommandHandler : IRequestHandler<SubmitSupervisionRequestCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubmitSupervisionRequestCommandHandler"/> class.
    /// </summary>
    /// <param name="context">The unit-of-work relational data access boundary layer.</param>
    /// <param name="currentUserService">Provides tracking visibility over the currently authenticated session security credentials.</param>
    /// <param name="dateTimeProvider">The deterministic system clock wrapper abstraction.</param>
    public SubmitSupervisionRequestCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    /// <summary>
    /// Validates session permissions, verifies entity existence, and submits the supervision request through the aggregate state machine.
    /// </summary>
    /// <param name="request">The incoming CQRS data payload containing workspace routing identifiers and pitch notes.</param>
    /// <param name="cancellationToken">The operational signal tracking asynchronous execution cancellations.</param>
    /// <returns>The generated primary tracking key Guid of the newly spawned supervision request entity log.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if either the target project instance or professor profile does not exist.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if an unauthenticated user or separate student attempts to alter the workspace.</exception>
    public async Task<Guid> Handle(SubmitSupervisionRequestCommand request, CancellationToken cancellationToken)
    {
        // 1. Retrieve the target project instance along with its current outstanding request collection
        var projectInstance = await _context.ProjectInstances
            .Include(pi => pi.SupervisionRequests)
            .FirstOrDefaultAsync(pi => pi.Id == request.ProjectInstanceId, cancellationToken);

        if (projectInstance == null)
        {
            throw new KeyNotFoundException($"The target project instance workspace with ID '{request.ProjectInstanceId}' was not found.");
        }

        // 2. Security Boundaries: Enforce that the current executing user session owns this specific workspace
        if (!_currentUserService.IsAuthenticated || projectInstance.StudentId != _currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("Access Denied: You can only submit academic supervision requests for project instances that you own.");
        }

        // 3. Relational Pre-Check: Verify that the designated professor profile exists within the university directory
        var professorExists = await _context.Professors
            .AnyAsync(p => p.Id == request.ProfessorId, cancellationToken);

        if (!professorExists)
        {
            throw new KeyNotFoundException($"The targeted academic professor profile with ID '{request.ProfessorId}' does not exist.");
        }

        // 4. Delegate core state transition checks and entity spawning downstream to the Aggregate Root
        // This execution automatically evaluates current lifecycle statuses and fires a SupervisionRequestCreatedEvent.
        projectInstance.SubmitSupervisionRequest(
            request.ProfessorId,
            request.PitchText,
            _dateTimeProvider.UtcNow);

        // 5. Commit state modifications down to physical relational rows atomically
        await _context.SaveChangesAsync(cancellationToken);

        // 6. Safely locate and extract the tracking identifier of the newly appended request entity within our boundary loop
        var newlyCreatedRequest = projectInstance.SupervisionRequests
            .First(r => r.Status == SupervisionRequestStatus.Pending);

        return newlyCreatedRequest.Id;
    }
}