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
/// appending a structural milestone, and persisting changes securely.
/// </summary>
public class AddGlobalMilestoneCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<AddGlobalMilestoneCommand, Guid>
{
    /// <summary>
    /// Handles the operational request to attach a new milestone onto a project template graph boundary securely.
    /// </summary>
    public async Task<Guid> Handle(AddGlobalMilestoneCommand request, CancellationToken cancellationToken)
    {
        // Enforce active session validation early before executing database logic
        if (!currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Access Denied: Authentication is mandatory to modify template configurations.");
        }

        // Fetch the parent template aggregate root.
        // Eagerly load the internal collection so the aggregate can manage its internal limits correctly.
        var template = await context.ProjectTemplates
            .Include(t => t.GlobalMilestones)
            .FirstOrDefaultAsync(t => t.Id == request.ProjectTemplateId, cancellationToken);

        // Validate aggregate presence and provider tenancy uniformly.
        // Using a single unified error boundary protects against side-channel resource enumeration vectors.
        if (template == null || template.ProviderId != currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("Access Denied: The requested project template was not found, or you do not possess management authorization permissions.");
        }

        // Delegate behavioral instantiation completely to the aggregate root boundary method
        template.AddMilestone(
            request.Title,
            request.Description,
            request.ExpectedEffortInHours,
            request.RequiredDeliverableType);

        // Commit alterations down to the database persistence layer
        await context.SaveChangesAsync(cancellationToken);

        // Avoid collection traversal assumptions by targeting the most recently appended structural tracking row matching the parameters
        var createdMilestone = template.GlobalMilestones
            .LastOrDefault(m => m.Title == request.Title.Trim() && m.ExpectedEffortInHours == request.ExpectedEffortInHours);

        if (createdMilestone == null)
        {
            throw new InvalidOperationException("An unexpected processing error occurred while materializing the template milestone tracking record.");
        }

        return createdMilestone.Id;
    }
}