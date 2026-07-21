using System;
using System.Threading;
using System.Threading.Tasks;
using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Common.Models.AiSync;
using AcademicGateway.Domain.Skills.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AcademicGateway.Application.Features.Skills.Events;

/// <summary>
/// Handles synchronization when a new skill entry is created.
/// </summary>
public class SkillCreatedSyncEventHandler(
    IApplicationDbContext dbContext,
    IAiMatchmakingClient aiClient,
    ILogger<SkillCreatedSyncEventHandler> logger)
    : IDomainEventHandler<SkillCreatedEvent>
{
    public async Task HandleAsync(SkillCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        await SkillSyncHelper.SyncSkillAsync(dbContext, aiClient, logger, domainEvent.SkillId, cancellationToken);
    }
}

/// <summary>
/// Handles synchronization when an existing skill entry is updated.
/// </summary>
public class SkillUpdatedSyncEventHandler(
    IApplicationDbContext dbContext,
    IAiMatchmakingClient aiClient,
    ILogger<SkillUpdatedSyncEventHandler> logger)
    : IDomainEventHandler<SkillUpdatedEvent>
{
    public async Task HandleAsync(SkillUpdatedEvent domainEvent, CancellationToken cancellationToken)
    {
        await SkillSyncHelper.SyncSkillAsync(dbContext, aiClient, logger, domainEvent.SkillId, cancellationToken);
    }
}

/// <summary>
/// Handles purging skill vector indexes when a skill entry is removed.
/// </summary>
public class SkillDeletedSyncEventHandler(
    IAiMatchmakingClient aiClient,
    ILogger<SkillDeletedSyncEventHandler> logger)
    : IDomainEventHandler<SkillDeletedEvent>
{
    public async Task HandleAsync(SkillDeletedEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation("Dispatching delete request for skill {SkillId} to AI Engine.", domainEvent.SkillId);
        await aiClient.DeleteSkillAsync(domainEvent.SkillId, cancellationToken);
    }
}

/// <summary>
/// Helper utility to construct Skill DTO payload and dispatch to the AI Client.
/// </summary>
internal static class SkillSyncHelper
{
    public static async Task SyncSkillAsync(
        IApplicationDbContext dbContext,
        IAiMatchmakingClient aiClient,
        ILogger logger,
        Guid skillId,
        CancellationToken cancellationToken)
    {
        var skill = await dbContext.Skills
            .FirstOrDefaultAsync(s => s.Id == skillId, cancellationToken);

        if (skill == null)
        {
            logger.LogWarning("Skill {SkillId} not found for AI synchronization.", skillId);
            return;
        }

        var model = new SkillSyncModel
        {
            Skill = new SkillPayload
            {
                Id = skill.Id,
                Name = skill.Name
            }
        };

        logger.LogInformation("Dispatching sync request for skill {SkillId} to AI Engine.", skill.Id);
        await aiClient.SyncSkillAsync(model, cancellationToken);
    }
}