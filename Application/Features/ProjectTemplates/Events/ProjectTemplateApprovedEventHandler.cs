using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Common.Models.AiSync;
using AcademicGateway.Domain.ProjectTemplates.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AcademicGateway.Application.Features.ProjectTemplates.Events;

/// <summary>
/// Orchestrates cross-aggregate application sync rules when a blueprint passes evaluation and becomes live.
/// Resolves human-readable text labels and dispatches the payload to the AI Matchmaking Engine.
/// </summary>
public class ProjectTemplateApprovedEventHandler(
    IApplicationDbContext dbContext,
    IAiMatchmakingClient aiClient,
    ILogger<ProjectTemplateApprovedEventHandler> logger)
    : IDomainEventHandler<ProjectTemplateApprovedEvent>
{
    public async Task HandleAsync(ProjectTemplateApprovedEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Project template {TemplateId} has been certified and approved. Synchronizing with AI Engine.",
            domainEvent.TemplateId);

        var template = await dbContext.ProjectTemplates
            .Include(pt => pt.ProjectTemplateSkills)
            .FirstOrDefaultAsync(pt => pt.Id == domainEvent.TemplateId, cancellationToken);

        if (template == null)
        {
            logger.LogWarning("Project template {TemplateId} not found for AI synchronization.", domainEvent.TemplateId);
            return;
        }

        var skillIds = template.ProjectTemplateSkills
            .Select(pts => pts.SkillId)
            .ToList();

        string? majorName = null;
        if (template.MajorId.HasValue)
        {
            majorName = await dbContext.Majors
                .Where(m => m.Id == template.MajorId.Value)
                .Select(m => m.Name)
                .FirstOrDefaultAsync(cancellationToken);
        }

        string? specialtyName = null;
        if (template.SpecialtyId.HasValue)
        {
            specialtyName = await dbContext.Specialties
                .Where(s => s.Id == template.SpecialtyId.Value)
                .Select(s => s.Name)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var skillNames = await dbContext.Skills
            .Where(s => skillIds.Contains(s.Id))
            .Select(s => s.Name)
            .ToListAsync(cancellationToken);

        var syncModel = new ProjectSyncModel
        {
            ProjectTemplate = new ProjectTemplatePayload
            {
                Id = template.Id,
                Title = template.Title,
                Description = template.Description,
                ProviderId = template.ProviderId,
                CreatedAt = template.CreatedAt,
                SkillIds = skillIds,
                MajorId = template.MajorId,
                SpecialtyId = template.SpecialtyId
            },
            MajorName = majorName,
            SpecialtyName = specialtyName,
            SkillNames = skillNames
        };

        await aiClient.SyncProjectAsync(syncModel, cancellationToken);
    }
}