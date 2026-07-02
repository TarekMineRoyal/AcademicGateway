using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.ProjectInstances;
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

    /// <summary>
    /// Initializes a new instance of the <see cref="StartProjectCommandHandler"/> class.
    /// </summary>
    /// <param name="context">The application data persistence context boundary.</param>
    /// <param name="dateTimeProvider">The deterministic system clock abstraction layer.</param>
    public StartProjectCommandHandler(IApplicationDbContext context, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
    }

    /// <summary>
    /// Processes the command to create a new live running project snapshot workspace.
    /// </summary>
    /// <param name="request">The structural data payload request details.</param>
    /// <param name="cancellationToken">The system thread cancellation monitor hook.</param>
    /// <returns>The newly generated tracking Guid assigned to the active workspace entity root.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the target source project template blueprint cannot be found.</exception>
    public async Task<Guid> Handle(StartProjectCommand request, CancellationToken cancellationToken)
    {
        // Fetch the source template from database tracking, including its required skill parameters
        var template = await _context.ProjectTemplates
            .Include(t => t.ProjectTemplateSkills)
            .FirstOrDefaultAsync(t => t.Id == request.TemplateId, cancellationToken);

        // Guard Invariant: Core template configuration must exist in the inventory system
        if (template == null)
        {
            throw new KeyNotFoundException($"The requested project template blueprint with ID '{request.TemplateId}' was not found.");
        }

        // Factory Method Execution (Prototype Pattern): Delegate instantiation down to the aggregate root
        var projectInstance = template.Instantiate(
            request.StudentId,
            _dateTimeProvider.UtcNow,
            request.RequestedProfessorId);

        // Append the brand new, unmanaged aggregate tracking block into our operational data stream
        _context.ProjectInstances.Add(projectInstance);

        // Commit all staging domain data changes down to physical database rows atomically
        await _context.SaveChangesAsync(cancellationToken);

        return projectInstance.Id;
    }
}