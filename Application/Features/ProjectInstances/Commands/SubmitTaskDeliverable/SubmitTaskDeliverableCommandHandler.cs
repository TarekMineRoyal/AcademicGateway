using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.ProjectInstances.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProjectInstances.Commands.SubmitTaskDeliverable;

/// <summary>
/// Orchestrates the application request pipeline for recording a student task deliverable submission.
/// Fetches the target project aggregate tree, passes the polymorphic payload to the domain layer for verification, 
/// and commits state updates atomically and securely.
/// </summary>
public class SubmitTaskDeliverableCommandHandler : IRequestHandler<SubmitTaskDeliverableCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubmitTaskDeliverableCommandHandler"/> class.
    /// </summary>
    /// <param name="context">The application data persistence context boundary interface.</param>
    /// <param name="dateTimeProvider">The deterministic system clock abstraction layer provider.</param>
    /// <param name="currentUserService">Provides tracking visibility over the currently authenticated session security credentials.</param>
    public SubmitTaskDeliverableCommandHandler(
        IApplicationDbContext context,
        IDateTimeProvider dateTimeProvider,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Processes the work submission command, ensuring multi-tier aggregate boundaries and polymorphic formatting invariants are respected securely.
    /// </summary>
    /// <param name="request">The incoming command model carrying the polymorphic payload data and target tracking identifiers.</param>
    /// <param name="cancellationToken">The asynchronous operation cancellation tracking token.</param>
    /// <returns>A MediatR completion compliance unit instance.</returns>
    /// <exception cref="UnauthorizedAccessException">Uniformly thrown if session authentication fails, resources don't exist, or tenancy fails validation.</exception>
    /// <exception cref="InvalidProjectInstanceTransitionException">Thrown if the project state blocks operational task submission updates.</exception>
    public async Task<Unit> Handle(SubmitTaskDeliverableCommand request, CancellationToken cancellationToken)
    {
        // Enforce active security session validation early before executing database logic
        if (!_currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Access Denied: Authentication is mandatory to submit task deliverables.");
        }

        // Architectural Necessity: To allow the aggregate root to locate the target milestone node and its nested tasks,
        // and evaluate submission invariants safely, we must explicitly eager-load the full multi-tier collection layout.
        var projectInstance = await _context.ProjectInstances
            .Include(p => p.LocalMilestones)
                .ThenInclude(m => m.LocalTasks)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectInstanceId, cancellationToken);

        // Validate aggregate presence and verify that the session user ID matches the assigned Student identity.
        // Using a single unified error boundary protects against side-channel resource enumeration vectors.
        if (projectInstance == null || projectInstance.StudentId != _currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("Access Denied: The requested project workspace was not found, or you do not possess task submission authorization permissions.");
        }

        // Pure DDD Orchestration: Expose a public routing endpoint on the Aggregate Root to pass the transaction 
        // down to its nested task entities, ensuring Application layers never manipulate child entities directly.
        projectInstance.SubmitTaskDeliverable(
            request.LocalMilestoneId,
            request.LocalTaskId,
            request.SubmissionPayload,
            _dateTimeProvider.UtcNow);

        // Commit all successfully verified data changes down to physical database rows atomically
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}