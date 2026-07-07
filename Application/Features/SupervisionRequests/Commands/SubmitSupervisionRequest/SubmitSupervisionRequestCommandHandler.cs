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
    /// Validates session permissions, verifies entity existence, and submits the supervision request securely.
    /// </summary>
    public async Task<Guid> Handle(SubmitSupervisionRequestCommand request, CancellationToken cancellationToken)
    {
        // 1. Enforce active session configuration early before reading data layers
        if (!_currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Access Denied: You must be authenticated to submit academic supervision requests.");
        }

        // 2. Load the target workspace aggregate root along with the required tracked collection graph
        var projectInstance = await _context.ProjectInstances
            .Include(pi => pi.SupervisionRequests)
            .FirstOrDefaultAsync(pi => pi.Id == request.ProjectInstanceId, cancellationToken);

        // 3. Protect against side-channel resource enumeration
        // Coalesce the null check and student ownership alignment verify checks into a single step.
        // Uniformly throw an UnauthorizedAccessException to completely mask resource presence from scanning mechanisms.
        if (projectInstance == null || projectInstance.StudentId != _currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("Access Denied: The requested project workspace was not found, or you do not possess ownership authorization permissions.");
        }

        // 4. Relational Pre-Check: Ensure the targeted professor profile exists within the registry boundaries
        var professorExists = await _context.Professors
            .AnyAsync(p => p.Id == request.ProfessorId, cancellationToken);

        if (!professorExists)
        {
            throw new KeyNotFoundException($"The targeted academic professor profile with ID '{request.ProfessorId}' does not exist within the institutional directory.");
        }

        // 5. Delegate execution down into the Aggregate Root boundary model method to respect pure DDD rules
        projectInstance.SubmitSupervisionRequest(
            request.ProfessorId,
            request.PitchText,
            _dateTimeProvider.UtcNow);

        // 6. Commit transactional modifications down to physical storage layout maps atomically
        await _context.SaveChangesAsync(cancellationToken);

        // 7. Avoid unsafe collection traversal assumptions (Checklist Metric #4)
        // Correlate the lookup parameters strictly against both the specific ProfessorId and Status context
        // to isolate the newly provisioned child entity configuration cleanly without picking up stale tracked items.
        var newlyCreatedRequest = projectInstance.SupervisionRequests
            .FirstOrDefault(r => r.ProfessorId == request.ProfessorId && r.Status == SupervisionRequestStatus.Pending);

        if (newlyCreatedRequest == null)
        {
            throw new InvalidOperationException("An unexpected processing error occurred while materializing the supervision invite tracking record.");
        }

        return newlyCreatedRequest.Id;
    }
}