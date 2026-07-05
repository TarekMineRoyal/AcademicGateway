using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.ProjectInstances;
using AcademicGateway.Domain.ProjectInstances.Services;
using AcademicGateway.Domain.ProjectTemplates;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGateway.Application.Features.ProjectInstances.Commands.StartProject;

/// <summary>
/// Handles the business orchestration loop for initializing a live project workspace from a blueprint template.
/// </summary>
public class StartProjectCommandHandler : IRequestHandler<StartProjectCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly LocalMilestoneFactory _localMilestoneFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="StartProjectCommandHandler"/> class.
    /// </summary>
    public StartProjectCommandHandler(
        IApplicationDbContext context,
        IDateTimeProvider dateTimeProvider,
        LocalMilestoneFactory localMilestoneFactory)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
        _localMilestoneFactory = localMilestoneFactory;
    }

    /// <summary>
    /// Processes the command to create a new live running project snapshot workspace.
    /// </summary>
    public async Task<Guid> Handle(StartProjectCommand request, CancellationToken cancellationToken)
    {
        // Fetch the source template from database tracking, including its full milestone graph topology
        var template = await _context.ProjectTemplates
            .Include(t => t.ProjectTemplateSkills)
            .Include(t => t.GlobalMilestones)
                .ThenInclude(m => m.InboundDependencies)
            .FirstOrDefaultAsync(t => t.Id == request.TemplateId, cancellationToken);

        if (template == null)
        {
            throw new KeyNotFoundException($"The requested project template blueprint with ID '{request.TemplateId}' was not found.");
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