using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Common.Models.AiSync;
using AcademicGateway.Domain.Professors.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AcademicGateway.Application.Features.Professors.Events;

/// <summary>
/// Handles synchronization when a new professor is registered.
/// </summary>
public class ProfessorRegisteredSyncEventHandler(
    IApplicationDbContext dbContext,
    IAiMatchmakingClient aiClient,
    ILogger<ProfessorRegisteredSyncEventHandler> logger)
    : IDomainEventHandler<ProfessorRegisteredEvent>
{
    public async Task HandleAsync(ProfessorRegisteredEvent domainEvent, CancellationToken cancellationToken)
    {
        await ProfessorSyncHelper.SyncProfessorAsync(dbContext, aiClient, logger, domainEvent.ProfessorId, cancellationToken);
    }
}

/// <summary>
/// Handles synchronization when an existing professor profile is updated.
/// </summary>
public class ProfessorUpdatedSyncEventHandler(
    IApplicationDbContext dbContext,
    IAiMatchmakingClient aiClient,
    ILogger<ProfessorUpdatedSyncEventHandler> logger)
    : IDomainEventHandler<ProfessorUpdatedEvent>
{
    public async Task HandleAsync(ProfessorUpdatedEvent domainEvent, CancellationToken cancellationToken)
    {
        await ProfessorSyncHelper.SyncProfessorAsync(dbContext, aiClient, logger, domainEvent.ProfessorId, cancellationToken);
    }
}

/// <summary>
/// Handles purging professor vector indexes when a professor profile is deleted.
/// </summary>
public class ProfessorDeletedSyncEventHandler(
    IAiMatchmakingClient aiClient,
    ILogger<ProfessorDeletedSyncEventHandler> logger)
    : IDomainEventHandler<ProfessorDeletedEvent>
{
    public async Task HandleAsync(ProfessorDeletedEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation("Dispatching delete request for professor {ProfessorId} to AI Engine.", domainEvent.ProfessorId);
        await aiClient.DeleteProfessorAsync(domainEvent.ProfessorId, cancellationToken);
    }
}

/// <summary>
/// Helper utility to resolve text labels and flatten Professor DTO payload.
/// </summary>
internal static class ProfessorSyncHelper
{
    public static async Task SyncProfessorAsync(
        IApplicationDbContext dbContext,
        IAiMatchmakingClient aiClient,
        ILogger logger,
        Guid professorId,
        CancellationToken cancellationToken)
    {
        var professor = await dbContext.Professors
            .Include(p => p.ResearchInterests)
            .ThenInclude(ri => ri.ResearchInterest)
            .FirstOrDefaultAsync(p => p.Id == professorId, cancellationToken);

        if (professor == null)
        {
            logger.LogWarning("Professor profile {ProfessorId} not found for AI synchronization.", professorId);
            return;
        }

        var interestIds = professor.ResearchInterests
            .Select(ri => ri.ResearchInterestId)
            .ToList();

        var interestAreas = professor.ResearchInterests
            .Where(ri => ri.ResearchInterest != null)
            .Select(ri => ri.ResearchInterest.Area)
            .ToList();

        var model = new ProfessorSyncModel
        {
            Professor = new ProfessorPayload
            {
                Id = professor.Id,
                FullName = professor.FullName,
                Department = professor.Department,
                Rank = professor.Rank,
                IsAcceptingProjects = professor.IsAcceptingProjects,
                ResearchInterestIds = interestIds,
                AboutMe = professor.AboutMe
            },
            InterestAreas = interestAreas
        };

        logger.LogInformation("Dispatching sync request for professor {ProfessorId} to AI Engine.", professor.Id);
        await aiClient.SyncProfessorAsync(model, cancellationToken);
    }
}