using AcademicGateway.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProjectInstances.Queries.GetProjectInstanceById;

/// <summary>
/// Handles the execution of the <see cref="GetProjectInstanceByIdQuery"/> request.
/// Leverages optimized, untracked relational database queries to extract live workspace operational contexts securely.
/// </summary>
public class GetProjectInstanceByIdQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetProjectInstanceByIdQuery, ProjectInstanceDetailDto?>
{
    /// <summary>
    /// Processes the live workspace deep-dive query, applying non-tracking projections across profile joins securely.
    /// </summary>
    /// <param name="request">The query container holding the primary lookup identifier key for the instance workspace.</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be canceled.</param>
    /// <returns>A detailed snapshot view data transfer object of the runtime workspace instance state.</returns>
    /// <exception cref="UnauthorizedAccessException">Uniformly thrown if session authentication fails, the resource is missing, or tenancy validation fails.</exception>
    public async Task<ProjectInstanceDetailDto?> Handle(GetProjectInstanceByIdQuery request, CancellationToken cancellationToken)
    {
        // Enforce active security session validation early before executing database logic
        if (!currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Access Denied: Authentication is mandatory to query project workspace details.");
        }

        // Query base: Deactivate object change-tracking overhead for maximum read-side execution performance
        var dto = await context.ProjectInstances
            .AsNoTracking()
            .Where(pi => pi.Id == request.Id)
            .Select(pi => new ProjectInstanceDetailDto
            {
                Id = pi.Id,
                StudentId = pi.StudentId,

                // Project the legal student name path safely by inspecting the student navigational linkage context
                StudentName = pi.Student != null ? pi.Student.FullName : "Unknown Student",

                SupervisorId = pi.SupervisorId,

                // Conditionally project the faculty supervisor's name if a mentor has actively bound to the workspace track
                SupervisorName = pi.Supervisor != null ? pi.Supervisor.FullName : null,

                TemplateId = pi.TemplateId,
                ProviderId = pi.ProviderId,
                TitleSnapshot = pi.TitleSnapshot,
                DescriptionSnapshot = pi.DescriptionSnapshot,
                Status = pi.Status,
                CreatedAt = pi.CreatedAt,
                EndDate = pi.EndDate,
                OverallGrade = pi.OverallGrade,
                ProjectGradedAt = pi.ProjectGradedAt,

                // Correlate the skill tracking keys directly against the master Skills context block
                // to pull down strings without depending on a non-existent navigation property.
                // Positional instantiation automatically maps ss.SkillId to InstanceSkillDto.Id
                SnapshotSkills = pi.SnapshotSkills.Select(ss => new InstanceSkillDto(
                    ss.SkillId,
                    context.Skills
                        .Where(s => s.Id == ss.SkillId)
                        .Select(s => s.Name)
                        .FirstOrDefault() ?? "Unknown Skill"
                )).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        // Validate aggregate presence and contextual user tenancy boundaries uniformly.
        // Access is strictly restricted to the participating student, supervisor, or provider.
        if (dto == null || (dto.StudentId != currentUserService.UserId &&
                            dto.SupervisorId != currentUserService.UserId &&
                            dto.ProviderId != currentUserService.UserId))
        {
            throw new UnauthorizedAccessException("Access Denied: The requested project workspace was not found, or you do not possess read authorization permissions.");
        }

        return dto;
    }
}