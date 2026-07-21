using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Common.Models.AiSync;
using AcademicGateway.Domain.ProjectTemplates.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AcademicGateway.Application.Features.Admin.Commands.ExecuteAiBackfill;

/// <summary>
/// Handles the execution of a bulk AI vector index backfill operation.
/// Eagerly loads domain entities, flattens relational data into DTO payloads,
/// and streams chunked payloads to the AI Matchmaking client in sequence.
/// </summary>
public class ExecuteAiBackfillCommandHandler(
    IApplicationDbContext dbContext,
    IAiMatchmakingClient aiClient,
    ILogger<ExecuteAiBackfillCommandHandler> logger)
    : IRequestHandler<ExecuteAiBackfillCommand, BackfillSummaryResult>
{
    /// <summary>
    /// Processes the bulk backfill request across specified domain entities in the required dependency sequence:
    /// Skills -> Professors -> Students -> Projects.
    /// </summary>
    /// <param name="request">The configuration options specifying entity flags and batch chunk sizes.</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be canceled.</param>
    /// <returns>A summary result tracking counts of synchronized entities and dispatched HTTP batches.</returns>
    public async Task<BackfillSummaryResult> Handle(ExecuteAiBackfillCommand request, CancellationToken cancellationToken)
    {
        var chunkSize = request.ChunkSize <= 0 ? 250 : request.ChunkSize;
        var summary = new BackfillSummaryResult();

        // 1. Skills Bulk Sync
        if (request.SyncSkills)
        {
            logger.LogInformation("Starting bulk AI backfill for Skills...");

            var skills = await dbContext.Skills
                .AsNoTracking()
                .Select(s => new SkillSyncModel
                {
                    Skill = new SkillPayload
                    {
                        Id = s.Id,
                        Name = s.Name
                    }
                })
                .ToListAsync(cancellationToken);

            foreach (var batch in skills.Chunk(chunkSize))
            {
                await aiClient.BulkSyncSkillsAsync(batch, cancellationToken);
                summary.SkillsSynced += batch.Length;
                summary.TotalBatchesDispatched++;
            }

            logger.LogInformation("Completed AI backfill for {Count} Skills across {Batches} batches.", summary.SkillsSynced, summary.TotalBatchesDispatched);
        }

        // 2. Professors Bulk Sync
        if (request.SyncProfessors)
        {
            logger.LogInformation("Starting bulk AI backfill for Professors...");
            var startBatchCount = summary.TotalBatchesDispatched;

            var professors = await dbContext.Professors
                .Include(p => p.ResearchInterests)
                    .ThenInclude(pri => pri.ResearchInterest)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var professorModels = professors.Select(p => new ProfessorSyncModel
            {
                Professor = new ProfessorPayload
                {
                    Id = p.Id,
                    FullName = p.FullName,
                    Department = p.Department,
                    Rank = p.Rank,
                    IsAcceptingProjects = p.IsAcceptingProjects,
                    ResearchInterestIds = p.ResearchInterests.Select(ri => ri.ResearchInterestId).ToList(),
                    AboutMe = p.AboutMe
                },
                InterestAreas = p.ResearchInterests
                    .Where(ri => ri.ResearchInterest != null)
                    .Select(ri => ri.ResearchInterest.Area)
                    .ToList()
            }).ToList();

            foreach (var batch in professorModels.Chunk(chunkSize))
            {
                await aiClient.BulkSyncProfessorsAsync(batch, cancellationToken);
                summary.ProfessorsSynced += batch.Length;
                summary.TotalBatchesDispatched++;
            }

            logger.LogInformation("Completed AI backfill for {Count} Professors across {Batches} batches.", summary.ProfessorsSynced, summary.TotalBatchesDispatched - startBatchCount);
        }

        // 3. Students Bulk Sync
        if (request.SyncStudents)
        {
            logger.LogInformation("Starting bulk AI backfill for Students...");
            var startBatchCount = summary.TotalBatchesDispatched;

            var students = await dbContext.Students
                .Include(s => s.StudentMajors)
                    .ThenInclude(sm => sm.Major)
                .Include(s => s.StudentSpecialties)
                    .ThenInclude(ss => ss.Specialty)
                .Include(s => s.StudentSkills)
                    .ThenInclude(ss => ss.Skill)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var studentModels = students.Select(s => new StudentSyncModel
            {
                Student = new StudentPayload
                {
                    Id = s.Id,
                    FullName = s.FullName,
                    MajorId = s.StudentMajors.Select(sm => (Guid?)sm.MajorId).FirstOrDefault(),
                    SpecialtyIds = s.StudentSpecialties.Select(ss => ss.SpecialtyId).ToList(),
                    SkillIds = s.StudentSkills.Select(ss => ss.SkillId).ToList(),
                    AboutMe = s.AboutMe
                },
                MajorName = s.StudentMajors.Select(sm => sm.Major.Name).FirstOrDefault(),
                SpecialtyNames = s.StudentSpecialties
                    .Where(ss => ss.Specialty != null)
                    .Select(ss => ss.Specialty.Name)
                    .ToList(),
                SkillNames = s.StudentSkills
                    .Where(ss => ss.Skill != null)
                    .Select(ss => ss.Skill.Name)
                    .ToList()
            }).ToList();

            foreach (var batch in studentModels.Chunk(chunkSize))
            {
                await aiClient.BulkSyncStudentsAsync(batch, cancellationToken);
                summary.StudentsSynced += batch.Length;
                summary.TotalBatchesDispatched++;
            }

            logger.LogInformation("Completed AI backfill for {Count} Students across {Batches} batches.", summary.StudentsSynced, summary.TotalBatchesDispatched - startBatchCount);
        }

        // 4. Project Templates Bulk Sync (FILTER INVARIANT: Approved ONLY)
        if (request.SyncProjects)
        {
            logger.LogInformation("Starting bulk AI backfill for Approved Project Templates...");
            var startBatchCount = summary.TotalBatchesDispatched;

            var majorsDict = await dbContext.Majors
                .AsNoTracking()
                .ToDictionaryAsync(m => m.Id, m => m.Name, cancellationToken);

            var specialtiesDict = await dbContext.Specialties
                .AsNoTracking()
                .ToDictionaryAsync(sp => sp.Id, sp => sp.Name, cancellationToken);

            var projectTemplates = await dbContext.ProjectTemplates
                .Where(pt => pt.Status == ProjectTemplateStatus.Approved)
                .Include(pt => pt.ProjectTemplateSkills)
                    .ThenInclude(pts => pts.Skill)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var projectModels = projectTemplates.Select(pt => new ProjectSyncModel
            {
                ProjectTemplate = new ProjectTemplatePayload
                {
                    Id = pt.Id,
                    Title = pt.Title,
                    Description = pt.Description,
                    ProviderId = pt.ProviderId,
                    CreatedAt = pt.CreatedAt,
                    SkillIds = pt.ProjectTemplateSkills.Select(pts => pts.SkillId).ToList(),
                    MajorId = pt.MajorId,
                    SpecialtyId = pt.SpecialtyId
                },
                MajorName = pt.MajorId.HasValue && majorsDict.TryGetValue(pt.MajorId.Value, out var majorName) ? majorName : null,
                SpecialtyName = pt.SpecialtyId.HasValue && specialtiesDict.TryGetValue(pt.SpecialtyId.Value, out var specName) ? specName : null,
                SkillNames = pt.ProjectTemplateSkills
                    .Where(pts => pts.Skill != null)
                    .Select(pts => pts.Skill.Name)
                    .ToList()
            }).ToList();

            foreach (var batch in projectModels.Chunk(chunkSize))
            {
                await aiClient.BulkSyncProjectsAsync(batch, cancellationToken);
                summary.ProjectsSynced += batch.Length;
                summary.TotalBatchesDispatched++;
            }

            logger.LogInformation("Completed AI backfill for {Count} Project Templates across {Batches} batches.", summary.ProjectsSynced, summary.TotalBatchesDispatched - startBatchCount);
        }

        return summary;
    }
}