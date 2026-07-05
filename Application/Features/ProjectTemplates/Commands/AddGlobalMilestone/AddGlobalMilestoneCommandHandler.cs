using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.ProjectTemplates.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProjectTemplates.Commands.AddGlobalMilestone;

/// <summary>
/// Orchestrates the application process for reading a target blueprint aggregate, 
/// appending a structural milestone, and persisting changes.
/// </summary>
public class AddGlobalMilestoneCommandHandler(IApplicationDbContext context)
    : IRequestHandler<AddGlobalMilestoneCommand, Guid>
{
    /// <summary>
    /// Handles the operational request to attach a new milestone onto a project template graph boundary.
    /// </summary>
    /// <param name="request">The incoming command container housing metadata parameters and parent tracking references.</param>
    /// <param name="cancellationToken">The operational token tracking asynchronous processing cancellations.</param>
    /// <returns>The unique surrogate tracking identifier assigned to the newly appended milestone configuration.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the target project template identifier is missing from database records.</exception>
    /// <exception cref="InvalidTemplateStatusException">Thrown if the aggregate root is in an immutable state (Approved/Rejected).</exception>
    /// <exception cref="InvalidTemplateDetailsException">Thrown if performance or boundary constraints fail invariants.</exception>
    public async Task<Guid> Handle(AddGlobalMilestoneCommand request, CancellationToken cancellationToken)
    {
        // Fetch the parent template aggregate root.
        // Eagerly load the internal collection so the aggregate can manage its internal limits correctly.
        var template = await context.ProjectTemplates
            .Include(t => t.GlobalMilestones)
            .FirstOrDefaultAsync(t => t.Id == request.ProjectTemplateId, cancellationToken);

        if (template == null)
        {
            throw new KeyNotFoundException($"Project Template with ID '{request.ProjectTemplateId}' was not found.");
        }

        // Delegate behavioral instantiation completely to the aggregate root boundary method
        template.AddMilestone(
            request.Title,
            request.Description,
            request.ExpectedEffortInHours,
            request.RequiredDeliverableType);

        // Commit alterations down to the database persistence layer
        await context.SaveChangesAsync(cancellationToken);

        // Locate the newly generated milestone entity from the tracking collection to return its tracking ID
        var createdMilestone = template.GlobalMilestones
            .First(m => m.Title == request.Title.Trim() && m.ExpectedEffortInHours == request.ExpectedEffortInHours);

        return createdMilestone.Id;
    }
}