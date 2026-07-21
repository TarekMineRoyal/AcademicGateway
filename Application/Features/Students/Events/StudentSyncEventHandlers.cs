using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Common.Models.AiSync;
using AcademicGateway.Domain.Students.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AcademicGateway.Application.Features.Students.Events;

/// <summary>
/// Handles synchronization when a new student is registered.
/// </summary>
public class StudentRegisteredSyncEventHandler(
    IApplicationDbContext dbContext,
    IAiMatchmakingClient aiClient,
    ILogger<StudentRegisteredSyncEventHandler> logger)
    : IDomainEventHandler<StudentRegisteredEvent>
{
    public async Task HandleAsync(StudentRegisteredEvent domainEvent, CancellationToken cancellationToken)
    {
        await StudentSyncHelper.SyncStudentAsync(dbContext, aiClient, logger, domainEvent.StudentId, cancellationToken);
    }
}

/// <summary>
/// Handles synchronization when an existing student profile is updated.
/// </summary>
public class StudentUpdatedSyncEventHandler(
    IApplicationDbContext dbContext,
    IAiMatchmakingClient aiClient,
    ILogger<StudentUpdatedSyncEventHandler> logger)
    : IDomainEventHandler<StudentUpdatedEvent>
{
    public async Task HandleAsync(StudentUpdatedEvent domainEvent, CancellationToken cancellationToken)
    {
        await StudentSyncHelper.SyncStudentAsync(dbContext, aiClient, logger, domainEvent.StudentId, cancellationToken);
    }
}

/// <summary>
/// Handles purging student vector indexes when a student profile is deleted.
/// </summary>
public class StudentDeletedSyncEventHandler(
    IAiMatchmakingClient aiClient,
    ILogger<StudentDeletedSyncEventHandler> logger)
    : IDomainEventHandler<StudentDeletedEvent>
{
    public async Task HandleAsync(StudentDeletedEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation("Dispatching delete request for student {StudentId} to AI Engine.", domainEvent.StudentId);
        await aiClient.DeleteStudentAsync(domainEvent.StudentId, cancellationToken);
    }
}

/// <summary>
/// Helper utility to resolve text labels and flatten Student DTO payload.
/// </summary>
internal static class StudentSyncHelper
{
    public static async Task SyncStudentAsync(
        IApplicationDbContext dbContext,
        IAiMatchmakingClient aiClient,
        ILogger logger,
        Guid studentId,
        CancellationToken cancellationToken)
    {
        var student = await dbContext.Students
            .Include(s => s.StudentMajors)
            .Include(s => s.StudentSpecialties)
            .Include(s => s.StudentSkills)
            .FirstOrDefaultAsync(s => s.Id == studentId, cancellationToken);

        if (student == null)
        {
            logger.LogWarning("Student profile {StudentId} not found for AI synchronization.", studentId);
            return;
        }

        var majorIds = student.StudentMajors.Select(sm => sm.MajorId).ToList();
        var primaryMajorId = majorIds.FirstOrDefault();

        var specialtyIds = student.StudentSpecialties.Select(ss => ss.SpecialtyId).ToList();
        var skillIds = student.StudentSkills.Select(ss => ss.SkillId).ToList();

        string? majorName = null;
        if (primaryMajorId != default)
        {
            majorName = await dbContext.Majors
                .Where(m => m.Id == primaryMajorId)
                .Select(m => m.Name)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var specialtyNames = await dbContext.Specialties
            .Where(s => specialtyIds.Contains(s.Id))
            .Select(s => s.Name)
            .ToListAsync(cancellationToken);

        var skillNames = await dbContext.Skills
            .Where(s => skillIds.Contains(s.Id))
            .Select(s => s.Name)
            .ToListAsync(cancellationToken);

        var model = new StudentSyncModel
        {
            Student = new StudentPayload
            {
                Id = student.Id,
                FullName = student.FullName,
                MajorId = primaryMajorId == default ? null : primaryMajorId,
                SpecialtyIds = specialtyIds,
                SkillIds = skillIds,
                AboutMe = student.AboutMe
            },
            MajorName = majorName,
            SpecialtyNames = specialtyNames,
            SkillNames = skillNames
        };

        logger.LogInformation("Dispatching sync request for student {StudentId} to AI Engine.", student.Id);
        await aiClient.SyncStudentAsync(model, cancellationToken);
    }
}