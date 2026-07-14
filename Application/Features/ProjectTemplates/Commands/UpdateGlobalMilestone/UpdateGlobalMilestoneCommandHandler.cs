using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.ProjectTemplates.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProjectTemplates.Commands.UpdateGlobalMilestone;

/// <summary>
/// Orchestrates the application process for retrieving a target project template aggregate, 
/// modifying an existing nested milestone node, and persisting changes securely.
/// </summary>
public class UpdateGlobalMilestoneCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<UpdateGlobalMilestoneCommand>
{
    /// <summary>
    /// Handles the operational request to update an existing milestone configuration within a project template graph boundary securely.
    /// </summary>
    public async Task Handle(UpdateGlobalMilestoneCommand request, CancellationToken cancellationToken)
    {
        // Enforce active session validation early before executing database logic
        if (!currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Access Denied: Authentication is mandatory to modify template configurations.");
        }

        // Fetch the parent template aggregate root.
        // Eagerly load the internal collection so the aggregate can manage its internal elements correctly.
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
        template.UpdateMilestone(
            request.MilestoneId,
            request.Title,
            request.Description,
            request.ExpectedEffortInHours,
            request.WbsWeight,
            request.GradingWeight);

        // Commit alterations down to the database persistence layer
        await context.SaveChangesAsync(cancellationToken);
    }
}