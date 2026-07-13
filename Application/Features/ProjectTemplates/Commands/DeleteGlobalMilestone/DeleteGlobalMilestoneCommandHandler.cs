using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.ProjectTemplates.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProjectTemplates.Commands.DeleteGlobalMilestone;

/// <summary>
/// Orchestrates the application process for retrieving a target project template aggregate, 
/// removing an existing nested milestone node along with its dependencies, and persisting changes securely.
/// </summary>
public class DeleteGlobalMilestoneCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<DeleteGlobalMilestoneCommand>
{
    /// <summary>
    /// Handles the operational request to permanently remove a milestone configuration from a project template graph boundary securely.
    /// </summary>
    public async Task Handle(DeleteGlobalMilestoneCommand request, CancellationToken cancellationToken)
    {
        // Enforce active session validation early before executing database logic
        if (!currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Access Denied: Authentication is mandatory to modify template configurations.");
        }

        // Fetch the parent template aggregate root.
        // Eagerly load the internal collection so the aggregate can manage its internal elements and handle dependency cascades correctly.
        var template = await context.ProjectTemplates
            .Include(t => t.GlobalMilestones)
            .FirstOrDefaultAsync(t => t.Id == request.ProjectTemplateId, cancellationToken);

        // Validate aggregate presence and provider tenancy uniformly.
        // Using a single unified error boundary protects against side-channel resource enumeration vectors.
        if (template == null || template.ProviderId != currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("Access Denied: The requested project template was not found, or you do not possess management authorization permissions.");
        }

        // Delegate behavioral state mutation completely to the aggregate root boundary method
        template.RemoveMilestone(request.MilestoneId);

        // Commit alterations down to the database persistence layer
        await context.SaveChangesAsync(cancellationToken);
    }
}