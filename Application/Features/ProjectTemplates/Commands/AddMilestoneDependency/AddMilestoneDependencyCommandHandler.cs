using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.ProjectTemplates.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProjectTemplates.Commands.AddMilestoneDependency;

/// <summary>
/// Orchestrates the command pipeline for building milestone relationships within an aggregate root boundary, 
/// ensuring topological constraints remain valid before database serialization.
/// Fortified against Broken Object Level Authorization (BOLA) and side-channel resource enumeration vectors.
/// </summary>
public class AddMilestoneDependencyCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<AddMilestoneDependencyCommand, Unit>
{
    /// <summary>
    /// Executes the relationship binding transaction across target nodes within the template domain boundary securely.
    /// </summary>
    public async Task<Unit> Handle(AddMilestoneDependencyCommand request, CancellationToken cancellationToken)
    {
        // Enforce active session validation early before executing database logic
        if (!currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Access Denied: Authentication is mandatory to modify template configurations.");
        }

        // Architectural Necessity: To correctly perform Directed Acyclic Graph (DAG) analysis inside memory, 
        // the handler must completely resolve the graph edges by eager-loading the entire milestone hierarchy, 
        // down through their inbound tracking constraint lists.
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
        // If a cycle or circular loop is introduced, the aggregate throws an InvalidOperationException 
        // immediately here, preventing corrupted state data changes from touching db tracking sessions.
        template.AddMilestoneDependency(
            request.SuccessorId,
            request.PredecessorId,
            request.Type);

        // Safely commit verified network structure updates down to persistence tables
        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}