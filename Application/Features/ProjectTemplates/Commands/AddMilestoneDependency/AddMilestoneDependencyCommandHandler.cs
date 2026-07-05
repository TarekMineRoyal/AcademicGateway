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
/// </summary>
public class AddMilestoneDependencyCommandHandler(IApplicationDbContext context)
    : IRequestHandler<AddMilestoneDependencyCommand, Unit>
{
    /// <summary>
    /// Executes the relationship binding transaction across target nodes within the template domain boundary.
    /// </summary>
    /// <param name="request">The incoming command container mapping node targets and structural edge behaviors.</param>
    /// <param name="cancellationToken">The operational signal monitoring asynchronous thread interruptions.</param>
    /// <returns>A MediatR validation compliance unit indicating successful transaction wrap-up.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the tracking parent template reference cannot be resolved.</exception>
    /// <exception cref="InvalidTemplateStatusException">Thrown if operations target locked aggregates.</exception>
    /// <exception cref="InvalidOperationException">Thrown if graph evaluation routines discover a Directed Acyclic Graph (DAG) cycle violation.</exception>
    public async Task<Unit> Handle(AddMilestoneDependencyCommand request, CancellationToken cancellationToken)
    {
        // Architectural Necessity: To correctly perform Directed Acyclic Graph (DAG) analysis inside memory, 
        // the handler must completely resolve the graph edges by eager-loading the entire milestone hierarchy, 
        // down through their inbound tracking constraint lists.
        var template = await context.ProjectTemplates
            .Include(t => t.GlobalMilestones)
                .ThenInclude(m => m.InboundDependencies)
            .FirstOrDefaultAsync(t => t.Id == request.ProjectTemplateId, cancellationToken);

        if (template == null)
        {
            throw new KeyNotFoundException($"Project Template with ID '{request.ProjectTemplateId}' was not found.");
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