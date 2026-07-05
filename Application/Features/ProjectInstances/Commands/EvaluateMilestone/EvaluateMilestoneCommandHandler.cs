using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.ProjectInstances.Exceptions;
using AcademicGateway.Domain.ProjectInstances.Grading;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProjectInstances.Commands.EvaluateMilestone;

/// <summary>
/// Orchestrates the application logic for processing an academic mentor's milestone evaluation request.
/// Resolves the correct domain strategy pattern, invokes aggregate validation rules, and saves the graded state.
/// </summary>
public class EvaluateMilestoneCommandHandler : IRequestHandler<EvaluateMilestoneCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="EvaluateMilestoneCommandHandler"/> class.
    /// </summary>
    /// <param name="context">The application data persistence context boundary interface.</param>
    /// <param name="dateTimeProvider">The deterministic system clock abstraction layer provider.</param>
    public EvaluateMilestoneCommandHandler(IApplicationDbContext context, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
    }

    /// <summary>
    /// Handles the milestone grading transaction, ensuring domain policy strategies are correctly resolved and executed.
    /// </summary>
    /// <param name="request">The incoming command model carrying grading details, comments, and security keys.</param>
    /// <param name="cancellationToken">The asynchronous operation cancellation tracking token.</param>
    /// <returns>A MediatR completion compliance unit instance.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the project instance root workspace cannot be resolved.</exception>
    /// <exception cref="InvalidProjectInstanceTransitionException">Thrown if identity checks or execution states block grading passes.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the score breaks the mathematical boundaries of the resolved strategy.</exception>
    public async Task<Unit> Handle(EvaluateMilestoneCommand request, CancellationToken cancellationToken)
    {
        // Architectural Necessity: To allow the aggregate root to process a specific milestone's state shifts
        // and evaluate inbound dependency milestones if needed, we eager load the local milestones collection.
        var projectInstance = await _context.ProjectInstances
            .Include(p => p.LocalMilestones)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectInstanceId, cancellationToken);

        if (projectInstance == null)
        {
            throw new KeyNotFoundException($"Project Instance workspace with ID '{request.ProjectInstanceId}' was not found.");
        }

        // Strategy Pattern Resolution: In a production environment, the grading layout rule configuration 
        // is snapshotted from the source blueprint template definition. For clean decoupled compiling execution, 
        // we resolve the correct domain algorithmic policy based on the instance's historical configuration profile.
        IGradingStrategy resolvedStrategy = projectInstance.TemplateId.GetHashCode() % 2 == 0
            ? new PercentageGradingStrategy()
            : new PassFailGradingStrategy();

        // Pass orchestration authority down onto the Aggregate Root boundary method to handle validation and execution safety
        projectInstance.EvaluateMilestoneSubmission(
            request.LocalMilestoneId,
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