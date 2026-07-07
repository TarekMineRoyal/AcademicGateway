using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.ProjectTemplates.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProjectTemplates.Commands.ResubmitProjectTemplate;

/// <summary>
/// Orchestrates the business command pipeline for modifying, re-validating, and transitioning a <see cref="ProjectTemplate"/> blueprint back into the evaluation loop.
/// Fortified against Broken Object Level Authorization (BOLA) and side-channel resource enumeration vectors.
/// </summary>
public class ResubmitProjectTemplateCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<ResubmitProjectTemplateCommand, Guid>
{
    /// <summary>
    /// Processes the template resubmission command request, managing internal skill alignment vectors and protecting state-machine boundaries cleanly.
    /// </summary>
    /// <param name="request">The CQRS command carrying the updated text details and skill criteria selection.</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be canceled.</param>
    /// <returns>The primary tracking identity key code of the resubmitted template snapshot blueprint.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown if session contexts fail authentication checks or cross-aggregate profile alignment fails.</exception>
    public async Task<Guid> Handle(ResubmitProjectTemplateCommand request, CancellationToken cancellationToken)
    {
        // 1. Enforce active security session validation early before executing database logic
        if (!currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Access Denied: Authentication is mandatory to generate or modify project template blueprints.");
        }

        // 2. Retrieve the target project template aggregate along with its tracking skill relational graph matrix
        var template = await context.ProjectTemplates
            .Include(t => t.ProjectTemplateSkills)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        // 3. Validate session boundary and prevent side-channel resource enumeration
        // If the template record is completely missing OR its ownership profile does not match the active corporate session,
        // throw a uniform authorization error to fully mask system resource presence from probing behavior.
        if (template == null || template.ProviderId != currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("Access Denied: The requested project template blueprint was not found, or you do not possess modifications authorization permissions.");
        }

        // 4. Delegate core descriptive updates over to the pure aggregate root domain methods
        // This ensures text scrubbing constraints and character formatting invariants are completely preserved inside the domain boundary.
        template.UpdateDetails(request.Title, request.Description);

        // 5. Synchronize technical skill competency matrices strictly using encapsulation behavioral gateways
        // Instead of executing direct multi-table database manipulations, we leverage the root entity to preserve full transactional cohesion.
        var currentSkillIds = template.ProjectTemplateSkills
            .Select(pts => pts.SkillId)
            .ToList();

        // Evict structural associations no longer required by the corrected layout specification
        foreach (var skillId in currentSkillIds)
        {
            template.RemoveSkill(skillId);
        }

        // Attach fresh operational requirements specified by the updating command payload
        if (request.SkillIds != null)
        {
            foreach (var skillId in request.SkillIds)
            {
                template.AddSkill(skillId);
            }
        }

        // 6. Advance the aggregate lifecycle track out of its static draft or feedback layout back into the review pool
        // This will natively fire internal state guards and append a ProjectTemplateSubmittedEvent onto the tracking queue.
        template.SubmitForReview();

        // 7. Atomically save additions and adjustments across all tracked aggregate boundaries
        await context.SaveChangesAsync(cancellationToken);

        // 8. Return the assigned surrogate tracking key identifier
        return template.Id;
    }
}