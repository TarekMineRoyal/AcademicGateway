using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.ProjectTemplates.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProjectTemplates.Commands.DeleteMilestoneDependency;

/// <summary>
/// Orchestrates the command pipeline for severing existing milestone relationships within an aggregate root boundary,
/// ensuring graph integrity remains intact before database serialization.
/// Fortified against Broken Object Level Authorization (BOLA) and side-channel resource enumeration vectors.
/// </summary>
public class DeleteMilestoneDependencyCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<DeleteMilestoneDependencyCommand, Unit>
{
    /// <summary>
    /// Executes the transaction to remove a specific dependency sequencing constraint link between target milestone nodes securely.
    /// </summary>
    public async Task<Unit> Handle(DeleteMilestoneDependencyCommand request, CancellationToken cancellationToken)
    {
        // Enforce active session validation early before executing database logic
        if (!currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Access Denied: Authentication is mandatory to modify template configurations.");
        }

        // Architectural Necessity: To correctly modify the dependency collection inside memory,
        // the handler must completely resolve the graph edges by eager-loading the milestone hierarchy,
        // down through their internal inbound tracking constraint lists.
        var template = await context.ProjectTemplates
            .Include(t => t.GlobalMilestones)
                .ThenInclude(m => m.InboundDependencies)
            .FirstOrDefaultAsync(t => t.Id == request.ProjectTemplateId, cancellationToken);

        // Validate aggregate presence and provider tenancy uniformly.
        // Using a single unified error boundary protects against side-channel resource enumeration vectors.
        if (template == null || template.ProviderId != currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("Access Denied: The requested project template was not found, or you do not possess management authorization permissions.");
        }

        // Forward execution requests directly to the aggregate root boundary method.
        // If the specified nodes or an active dependency edge connecting them cannot be found,
        // an InvalidTemplateDetailsException is thrown natively from the aggregate root context.
        template.RemoveMilestoneDependency(
            request.SuccessorId,
            request.PredecessorId);

        // Safely commit verified network structure updates down to persistence tables
        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}