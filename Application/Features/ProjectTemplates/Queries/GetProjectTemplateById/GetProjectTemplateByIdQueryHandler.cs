using AcademicGateway.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProjectTemplates.Queries.GetProjectTemplateById;

/// <summary>
/// Handles the execution of the <see cref="GetProjectTemplateByIdQuery"/> request.
/// Employs clean, untracked relational projection patterns to gather deep configuration graphs.
/// </summary>
public class GetProjectTemplateByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetProjectTemplateByIdQuery, ProjectTemplateDetailDto?>
{
    /// <summary>
    /// Processes the detailed blueprint deep-dive retrieval query, mapping full relational node matrices.
    /// </summary>
    /// <param name="request">The query container containing the target aggregate root identifier key.</param>
    /// <param name="cancellationToken">Propagates notification that operational threads should be canceled.</param>
    /// <returns>A detailed snapshot data transfer object of the template configuration, or null if not found.</returns>
    public async Task<ProjectTemplateDetailDto?> Handle(GetProjectTemplateByIdQuery request, CancellationToken cancellationToken)
    {
        // Query base: Deactivate object change-tracking for maximum read-side processing performance
        return await context.ProjectTemplates
            .AsNoTracking()
            .Where(t => t.Id == request.Id)
            .Select(t => new ProjectTemplateDetailDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                ProviderId = t.ProviderId,

                // Map out the skill prerequisite join collection
                RequiredSkills = t.ProjectTemplateSkills.Select(pts => new TemplateSkillDto(
                    pts.SkillId,
                    pts.Skill != null ? pts.Skill.Name : "Unknown Skill"
                )).ToList(),

                // Map out the core milestone nodes configured on this blueprint
                Milestones = t.GlobalMilestones.Select(gm => new TemplateGlobalMilestoneDto(
                    gm.Id,
                    gm.Title,
                    gm.Description,
                    gm.ExpectedEffortInHours,
                    gm.RequiredDeliverableType
                )).ToList(),

                // Flatten out the directed dependency graph matrix edges from all milestones combined
                Dependencies = t.GlobalMilestones
                    .SelectMany(gm => gm.InboundDependencies)
                    .Select(dep => new TemplateMilestoneDependencyDto(
                        dep.PredecessorId,
                        dep.SuccessorId,
                        dep.Type
                    )).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}