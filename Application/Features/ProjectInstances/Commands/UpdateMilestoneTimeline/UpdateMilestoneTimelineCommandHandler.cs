using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.ProjectInstances.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProjectInstances.Commands.UpdateMilestoneTimeline;

/// <summary>
/// Orchestrates the application logic for updating a local milestone's scheduled operational timeline.
/// Fetches the target project aggregate, executes structural graph sequence checks, and persists shifts securely.
/// </summary>
public class UpdateMilestoneTimelineCommandHandler : IRequestHandler<UpdateMilestoneTimelineCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateMilestoneTimelineCommandHandler"/> class.
    /// </summary>
    /// <param name="context">The application data persistence context boundary interface.</param>
    /// <param name="currentUserService">Provides tracking visibility over the currently authenticated session security credentials.</param>
    public UpdateMilestoneTimelineCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Processes the timeline scheduling update request, ensuring the graph remains compliant with constraint rules securely.
    /// </summary>
    /// <param name="request">The incoming command model carrying timeline adjustments and destination identifiers.</param>
    /// <param name="cancellationToken">The asynchronous operation cancellation tracking token.</param>
    /// <returns>A MediatR completion compliance unit instance.</returns>
    /// <exception cref="UnauthorizedAccessException">Uniformly thrown if session authentication fails, resources don't exist, or tenancy fails validation.</exception>
    public async Task<Unit> Handle(UpdateMilestoneTimelineCommand request, CancellationToken cancellationToken)
    {
        // Enforce active security session validation early before executing database logic
        if (!_currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Access Denied: Authentication is mandatory to adjust milestone timelines.");
        }

        // Architectural Necessity: To ensure that the internal aggregate validation engine can audit the 
        // entire execution topology accurately, the handler must completely eager-load all local milestone 
        // elements alongside their full inbound tracking dependency constraint configurations.
        var projectInstance = await _context.ProjectInstances
            .Include(p => p.LocalMilestones)
                .ThenInclude(m => m.InboundDependencies)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectInstanceId, cancellationToken);

        // Validate aggregate presence and verify contextual user tenancy parameters uniformly.
        // Restricting workflow adjustments exclusively to participating workspace profiles masks resource presence indicators.
        if (projectInstance == null || (projectInstance.StudentId != _currentUserService.UserId &&
                                        projectInstance.SupervisorId != _currentUserService.UserId &&
                                        projectInstance.ProviderId != _currentUserService.UserId))
        {
            throw new UnauthorizedAccessException("Access Denied: The requested project workspace was not found, or you do not possess milestone management authorization permissions.");
        }

        // Delegate timeline alterations and full graph chronological validation directly to the aggregate root boundary method
        projectInstance.UpdateMilestoneTimeline(
            request.LocalMilestoneId,
            request.ScheduledStartDate,
            request.ScheduledEndDate);

        // Commit all successfully validated scheduling updates back down to physical storage layout layers
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}