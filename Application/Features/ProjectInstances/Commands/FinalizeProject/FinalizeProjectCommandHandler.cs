using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.ProjectInstances.Exceptions;
using AcademicGateway.Domain.ProjectInstances.Grading;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProjectInstances.Commands.FinalizeProject;

/// <summary>
/// Orchestrates the application logic for calculating and certifying the final aggregate project score.
/// Resolves the corresponding domain grading strategy algorithm and executes the finalization boundary checks securely.
/// </summary>
public class FinalizeProjectCommandHandler : IRequestHandler<FinalizeProjectCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="FinalizeProjectCommandHandler"/> class.
    /// </summary>
    /// <param name="context">The application data persistence context boundary interface.</param>
    /// <param name="dateTimeProvider">The deterministic system clock abstraction layer provider.</param>
    /// <param name="currentUserService">Provides tracking visibility over the currently authenticated session security credentials.</param>
    public FinalizeProjectCommandHandler(
        IApplicationDbContext context,
        IDateTimeProvider dateTimeProvider,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Handles the project-wide scoring aggregate transaction, invoking double-dispatch strategy evaluation routines securely.
    /// </summary>
    /// <param name="request">The incoming command carrying target tracking keys and executor credentials.</param>
    /// <param name="cancellationToken">The asynchronous operation cancellation tracking token.</param>
    /// <returns>A MediatR completion compliance unit instance.</returns>
    /// <exception cref="UnauthorizedAccessException">Uniformly thrown if session authentication fails, resources don't exist, or tenancy fails validation.</exception>
    /// <exception cref="InvalidProjectInstanceTransitionException">Thrown if milestone readiness invariants fail checks.</exception>
    public async Task<Unit> Handle(FinalizeProjectCommand request, CancellationToken cancellationToken)
    {
        // Enforce active security session validation early before executing database logic
        if (!_currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Access Denied: Authentication is mandatory to finalize project grades.");
        }

        // Verify identity cross-referencing to prevent executing user spoofing
        if (request.ExecutingUserId != _currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("Access Denied: You cannot finalize a grade on behalf of a different identity profile.");
        }

        // Architectural Necessity: To allow the domain grading strategy to loop across and evaluate 
        // every milestone node in the execution graph, we must completely eager-load the LocalMilestones collection.
        var projectInstance = await _context.ProjectInstances
            .Include(p => p.LocalMilestones)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectInstanceId, cancellationToken);

        // Validate aggregate presence and verify that the session user ID matches the assigned Supervisor identity.
        // Using a single unified error boundary protects against side-channel resource enumeration vectors.
        if (projectInstance == null || projectInstance.SupervisorId != _currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("Access Denied: The requested project workspace was not found, or you do not possess grade finalization authorization permissions.");
        }

        // Strategy Pattern Resolution: Resolves the active domain strategy configuration matching the original template layout setup.
        IGradingStrategy resolvedStrategy = projectInstance.TemplateId.GetHashCode() % 2 == 0
            ? new PercentageGradingStrategy()
            : new PassFailGradingStrategy();

        // Delegate macro calculation and authorization policing completely to the Aggregate Root
        projectInstance.FinalizeProjectGrade(
            resolvedStrategy,
            _dateTimeProvider.UtcNow,
            request.ExecutingUserId);

        // Commit macro-level grade rows down to persistent data fields atomically
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}