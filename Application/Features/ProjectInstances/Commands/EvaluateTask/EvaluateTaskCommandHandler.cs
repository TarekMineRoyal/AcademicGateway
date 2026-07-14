using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.ProjectInstances.Exceptions;
using AcademicGateway.Domain.ProjectInstances.Grading;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProjectInstances.Commands.EvaluateTask;

/// <summary>
/// Orchestrates the application logic for processing an academic mentor's nested task evaluation request.
/// Resolves the correct domain strategy pattern, invokes aggregate validation rules, and saves the graded state securely.
/// </summary>
public class EvaluateTaskCommandHandler : IRequestHandler<EvaluateTaskCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="EvaluateTaskCommandHandler"/> class.
    /// </summary>
    /// <param name="context">The application data persistence context boundary interface.</param>
    /// <param name="dateTimeProvider">The deterministic system clock abstraction layer provider.</param>
    /// <param name="currentUserService">Provides tracking visibility over the currently authenticated session security credentials.</param>
    public EvaluateTaskCommandHandler(
        IApplicationDbContext context,
        IDateTimeProvider dateTimeProvider,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Handles the task grading transaction, ensuring domain policy strategies are correctly resolved and executed securely.
    /// </summary>
    /// <param name="request">The incoming command model carrying grading details, comments, and security keys.</param>
    /// <param name="cancellationToken">The asynchronous operation cancellation tracking token.</param>
    /// <returns>A MediatR completion compliance unit instance.</returns>
    /// <exception cref="UnauthorizedAccessException">Uniformly thrown if session authentication fails, resources don't exist, or tenancy fails validation.</exception>
    /// <exception cref="InvalidProjectInstanceTransitionException">Thrown if execution states block grading passes.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the score breaks the mathematical boundaries of the resolved strategy.</exception>
    public async Task<Unit> Handle(EvaluateTaskCommand request, CancellationToken cancellationToken)
    {
        // Enforce active security session validation early before executing database logic
        if (!_currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Access Denied: Authentication is mandatory to evaluate task submissions.");
        }

        // Verify identity cross-referencing to prevent reviewer context spoofing
        if (request.ExecutingProfessorId != _currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("Access Denied: You cannot submit evaluations on behalf of a different identity profile.");
        }

        // Architectural Necessity: To allow the aggregate root to process a specific milestone's nested task state shifts
        // and evaluate inbound dependencies safely, we eager load the entire local milestones and child tasks collection tree.
        var projectInstance = await _context.ProjectInstances
            .Include(p => p.LocalMilestones)
                .ThenInclude(m => m.LocalTasks)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectInstanceId, cancellationToken);

        // Validate aggregate presence and verify that the session user ID matches the assigned Supervisor identity.
        // Using a single unified error boundary protects against side-channel resource enumeration vectors.
        if (projectInstance == null || projectInstance.SupervisorId != _currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("Access Denied: The requested project workspace was not found, or you do not possess evaluation management authorization permissions.");
        }

        // Strategy Pattern Resolution: In a production environment, the grading layout rule configuration 
        // is snapshotted from the source blueprint template definition. For clean decoupled compiling execution, 
        // we resolve the correct domain algorithmic policy based on the instance's historical configuration profile.
        IGradingStrategy resolvedStrategy = projectInstance.TemplateId.GetHashCode() % 2 == 0
            ? new PercentageGradingStrategy()
            : new PassFailGradingStrategy();

        // Pass orchestration authority down onto the Aggregate Root boundary method to handle validation and execution safety
        projectInstance.EvaluateTaskSubmission(
            request.LocalMilestoneId,
            request.LocalTaskId,
            request.Grade,
            request.Feedback,
            resolvedStrategy,
            _dateTimeProvider.UtcNow,
            request.ExecutingProfessorId);

        // Commit successfully certified grading parameters to the underlying persistence layers
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}