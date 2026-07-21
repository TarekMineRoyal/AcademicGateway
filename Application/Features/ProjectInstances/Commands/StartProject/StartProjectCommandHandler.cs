using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.ProjectInstances;
using AcademicGateway.Domain.ProjectInstances.Enums;
using AcademicGateway.Domain.ProjectInstances.Services;
using AcademicGateway.Domain.ProjectTemplates;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGateway.Application.Features.ProjectInstances.Commands.StartProject;

/// <summary>
/// Handles the business orchestration loop for initializing a live project workspace from a blueprint template securely.
/// </summary>
public class StartProjectCommandHandler : IRequestHandler<StartProjectCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly LocalMilestoneFactory _localMilestoneFactory;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="StartProjectCommandHandler"/> class.
    /// </summary>
    public StartProjectCommandHandler(
        IApplicationDbContext context,
        IDateTimeProvider dateTimeProvider,
        LocalMilestoneFactory localMilestoneFactory,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
        _localMilestoneFactory = localMilestoneFactory;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Processes the command to create a new live running project snapshot workspace securely.
    /// </summary>
    /// <param name="request">The command parameter payload detailing the blueprint template identity and assigned profiles.</param>
    /// <param name="cancellationToken">Propagates notification that network execution threads should be canceled.</param>
    /// <returns>A unique tracking identifier primary key assigned onto the newly initialized project instance.</returns>
    /// <exception cref="UnauthorizedAccessException">Uniformly thrown if session authentication fails, resources don't exist, or tenancy fails validation.</exception>
    /// <exception cref="InvalidOperationException">Thrown if a duplicate active project instance invariant check fails.</exception>
    public async Task<Guid> Handle(StartProjectCommand request, CancellationToken cancellationToken)
    {
        // Enforce active security session validation early before executing database logic
        if (!_currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Access Denied: Authentication is mandatory to initialize project workspaces.");
        }

        // Verify identity cross-referencing to ensure users can only start project lifecycles for themselves
        if (request.StudentId != _currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("Access Denied: You cannot initialize a project workspace on behalf of a different identity profile.");
        }

        // Guard Invariant: Prevent duplicate concurrent active workspace tracks for the exact same project blueprint template.
        // It validates that the student does not have any active pipeline instance currently running or awaiting supervisor approval.
        bool alreadyActive = await _context.ProjectInstances
            .AnyAsync(pi => pi.StudentId == request.StudentId
                         && pi.TemplateId == request.TemplateId
                         && pi.Status != ProjectInstanceStatus.Concluded
                         && pi.Status != ProjectInstanceStatus.Canceled,
                      cancellationToken);

        if (alreadyActive)
        {
            // Substitutes for a custom BadRequestException if required by your pipeline architecture conventions
            throw new InvalidOperationException("Conflict: You already possess an active or pending workspace track for this project blueprint.");
        }

        // Fetch the source template from database tracking, including its full milestone graph topology
        var template = await _context.ProjectTemplates
            .Include(t => t.ProjectTemplateSkills)
            .Include(t => t.GlobalMilestones)
                .ThenInclude(m => m.InboundDependencies)
            .Include(t => t.GlobalMilestones)
                .ThenInclude(m => m.GlobalTasks)
            .FirstOrDefaultAsync(t => t.Id == request.TemplateId, cancellationToken);

        // Validate template presence boundaries uniformly to protect against resource scanning behaviors
        if (template == null)
        {
            throw new UnauthorizedAccessException("Access Denied: The requested project template was not found, or you do not possess blueprint initialization permissions.");
        }

        // Pure DDD Execution: The aggregate method accepts the domain service and outputs a fully formed, completely valid aggregate root instance.
        var projectInstance = template.Instantiate(
            request.StudentId,
            _dateTimeProvider.UtcNow,
            _localMilestoneFactory,
            request.RequestedProfessorId);

        // Append the new aggregate tracking block into our operational data stream context
        _context.ProjectInstances.Add(projectInstance);

        // Commit all changes down to physical database rows atomically
        await _context.SaveChangesAsync(cancellationToken);

        return projectInstance.Id;
    }
}