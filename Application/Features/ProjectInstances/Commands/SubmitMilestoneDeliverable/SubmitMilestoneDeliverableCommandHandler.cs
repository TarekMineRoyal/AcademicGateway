using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.ProjectInstances.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProjectInstances.Commands.SubmitMilestoneDeliverable;

/// <summary>
/// Orchestrates the application request pipeline for recording a student milestone deliverable submission.
/// Fetches the target project aggregate, passes the polymorphic payload to the domain layer for verification, 
/// and commits state updates atomically.
/// </summary>
public class SubmitMilestoneDeliverableCommandHandler : IRequestHandler<SubmitMilestoneDeliverableCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubmitMilestoneDeliverableCommandHandler"/> class.
    /// </summary>
    /// <param name="context">The application data persistence context boundary interface.</param>
    /// <param name="dateTimeProvider">The deterministic system clock abstraction layer provider.</param>
    public SubmitMilestoneDeliverableCommandHandler(IApplicationDbContext context, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
    }

    /// <summary>
    /// Processes the work submission command, ensuring aggregate boundaries and polymorphic formatting invariants are respected.
    /// </summary>
    /// <param name="request">The incoming command model carrying the polymorphic payload data and target tracking identifiers.</param>
    /// <param name="cancellationToken">The asynchronous operation cancellation tracking token.</param>
    /// <returns>A MediatR completion compliance unit instance.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the target project instance workspace cannot be resolved.</exception>
    /// <exception cref="InvalidProjectInstanceTransitionException">Thrown if the project state blocks operational task submission updates.</exception>
    /// <exception cref="InvalidOperationException">Thrown if formatting constraints or lifecycle rule checks fail inside the domain boundary.</exception>
    public async Task<Unit> Handle(SubmitMilestoneDeliverableCommand request, CancellationToken cancellationToken)
    {
        // Architectural Necessity: To allow the aggregate root to locate the target milestone node 
        // and evaluate its submission invariants safely, we must explicitly eager-load the local milestones collection.
        var projectInstance = await _context.ProjectInstances
            .Include(p => p.LocalMilestones)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectInstanceId, cancellationToken);

        if (projectInstance == null)
        {
            throw new KeyNotFoundException($"Project Instance workspace with ID '{request.ProjectInstanceId}' was not found.");
        }

        // Pure DDD Orchestration: Expose a public routing endpoint on the Aggregate Root to pass the transaction 
        // down to its internal milestone entities, ensuring Application layers never manipulate child entities directly.
        // Note: We will append this routing method to ProjectInstance next to keep assemblies compiling cleanly.
        projectInstance.SubmitMilestoneDeliverable(
            request.LocalMilestoneId,
            request.SubmissionPayload,
            _dateTimeProvider.UtcNow);

        // Commit all successfully verified data changes down to physical database rows atomically
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}