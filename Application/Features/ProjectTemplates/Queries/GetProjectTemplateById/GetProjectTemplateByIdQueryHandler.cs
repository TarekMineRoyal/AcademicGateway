using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.Common.Constants;
using AcademicGateway.Domain.ProjectTemplates.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProjectTemplates.Queries.GetProjectTemplateById;

/// <summary>
/// Handles the execution of the <see cref="GetProjectTemplateByIdQuery"/> request.
/// Employs clean, untracked relational projection patterns to gather deep configuration graphs securely.
/// </summary>
public class GetProjectTemplateByIdQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetProjectTemplateByIdQuery, ProjectTemplateDetailDto?>
{
    /// <summary>
    /// Processes the detailed blueprint deep-dive retrieval query, mapping full relational node matrices securely.
    /// </summary>
    /// <param name="request">The query container containing the target aggregate root identifier key.</param>
    /// <param name="cancellationToken">Propagates notification that operational threads should be canceled.</param>
    /// <returns>A detailed snapshot data transfer object of the template configuration, or null if not found.</returns>
    public async Task<ProjectTemplateDetailDto?> Handle(GetProjectTemplateByIdQuery request, CancellationToken cancellationToken)
    {
        // 1. Enforce active security session validation early before executing database logic
        if (!currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Access Denied: Authentication is mandatory to query template configurations.");
        }

        // 2. Query base: Deactivate object change-tracking for maximum read-side processing performance
        var dto = await context.ProjectTemplates
            .AsNoTracking()
            .Where(t => t.Id == request.Id)
            .Select(t => new ProjectTemplateDetailDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                ProviderId = t.ProviderId,
                MajorId = t.MajorId,
                SpecialtyId = t.SpecialtyId,
                MajorName = t.MajorId != null ? context.Majors.Where(m => m.Id == t.MajorId).Select(m => m.Name).FirstOrDefault() : null,
                SpecialtyName = t.SpecialtyId != null ? context.Specialties.Where(s => s.Id == t.SpecialtyId).Select(s => s.Name).FirstOrDefault() : null,

                // Map out the skill prerequisite join collection
                RequiredSkills = t.ProjectTemplateSkills.Select(pts => new TemplateSkillDto(
                    pts.SkillId,
                    pts.Skill != null ? pts.Skill.Name : "Unknown Skill"
                )).ToList(),

                // Map out the core milestone containers and their nested hierarchical tasks tree
                Milestones = t.GlobalMilestones.Select(gm => new TemplateGlobalMilestoneDto
                {
                    Id = gm.Id,
                    Title = gm.Title,
                    Description = gm.Description,
                    ExpectedEffortInHours = gm.ExpectedEffortInHours,
                    WbsWeight = gm.WbsWeight,
                    GradingWeight = gm.GradingWeight,
                    Tasks = gm.GlobalTasks.Select(gt => new GlobalTaskDto(
                        gt.Id,
                        gt.Title,
                        gt.Description,
                        gt.Weight,
                        gt.RequiredDeliverableType
                    )).ToList()
                }).ToList(),

                // Flatten out the directed execution graph matrix edges from all milestones combined
                Dependencies = t.GlobalMilestones
                    .SelectMany(gm => gm.InboundDependencies)
                    .Select(dep => new TemplateMilestoneDependencyDto(
                        dep.PredecessorId,
                        dep.SuccessorId,
                        dep.Type
                    )).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        // 3. Resource existence guard: Return null so the controller handles a clean HTTP 404 Not Found response
        if (dto == null)
        {
            return null;
        }

        // 4. Evaluate Visibility Matrix:
        // Access is authorized if:
        //   - User is the authoring provider (isOwner)
        //   - Template is publicly approved (isPubliclyAvailable)
        //   - User has administrative or reviewer privileges (isReviewer)
        bool isOwner = dto.ProviderId == currentUserService.UserId;
        bool isPubliclyAvailable = dto.Status == ProjectTemplateStatus.Approved;
        bool isReviewer = currentUserService.IsInRole(Roles.Reviewer) || currentUserService.IsInRole(Roles.Admin);

        if (!isOwner && !isPubliclyAvailable && !isReviewer)
        {
            throw new UnauthorizedAccessException("Access Denied: You do not possess read authorization permissions for this template blueprint.");
        }

        return dto;
    }
}