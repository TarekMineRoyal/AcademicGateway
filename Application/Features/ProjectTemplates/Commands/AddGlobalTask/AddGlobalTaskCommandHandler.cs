using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.ProjectTemplates.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProjectTemplates.Commands.AddGlobalTask;

/// <summary>
/// Orchestrates the application process for loading a target template aggregate graph, 
/// routing task appendations securely through domain boundaries, and persisting changes.
/// </summary>
public class AddGlobalTaskCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<AddGlobalTaskCommand, Guid>
{
    /// <summary>
    /// Handles the operational request to attach a nested task blueprint onto an internal milestone container securely.
    /// </summary>
    public async Task<Guid> Handle(AddGlobalTaskCommand request, CancellationToken cancellationToken)
    {
        // Enforce active session validation early before executing database logic
        if (!currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Access Denied: Authentication is mandatory to modify template configurations.");
        }

        // Fetch the parent template aggregate root.
        // Eagerly load the internal milestone collection along with child tasks to maintain full encapsulation boundaries.
        var template = await context.ProjectTemplates
            .Include(t => t.GlobalMilestones)
                .ThenInclude(m => m.GlobalTasks)
            .FirstOrDefaultAsync(t => t.Id == request.ProjectTemplateId, cancellationToken);

        // Validate aggregate presence and provider tenancy uniformly.
        // Using a single unified error boundary protects against side-channel resource enumeration vectors.
        if (template == null || template.ProviderId != currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("Access Denied: The requested project template was not found, or you do not possess management authorization permissions.");
        }

        // Route initialization behavior entirely through the aggregate root router boundary method
        template.AddGlobalTaskToMilestone(
            request.GlobalMilestoneId,
            request.Title,
            request.Description,
            request.Weight,
            request.RequiredDeliverableType);

        // Commit alterations down to the database persistence layer
        await context.SaveChangesAsync(cancellationToken);

        // Resolve the specific milestone container inside the reloaded snapshot graph context
        var targetMilestone = template.GlobalMilestones
            .FirstOrDefault(m => m.Id == request.GlobalMilestoneId);

        if (targetMilestone == null)
        {
            throw new InvalidOperationException("The target milestone container could not be found within the template context layout.");
        }

        // Locate the tracking entity configuration entry by targeting the most recently appended row matching metadata criteria
        var createdTask = targetMilestone.GlobalTasks
            .LastOrDefault(t => t.Title == request.Title.Trim()
                             && t.Weight == request.Weight
                             && t.RequiredDeliverableType == request.RequiredDeliverableType);

        if (createdTask == null)
        {
            throw new InvalidOperationException("An unexpected processing error occurred while materializing the template task tracking record.");
        }

        return createdTask.Id;
    }
}