using AcademicGateway.Application.Common.Interfaces;
using Domain.Lookups;
using Domain.ProjectTemplates;
using Domain.Providers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProjectTemplates.Commands.CreateTemplate;

public class CreateProjectTemplateCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateProjectTemplateCommand, Guid>
{
    public async Task<Guid> Handle(CreateProjectTemplateCommand request, CancellationToken cancellationToken)
    {
        // 1. Retrieve the requesting Provider and enforce the platform verification rule
        var provider = await context.Providers
            .FirstOrDefaultAsync(p => p.Id == request.ProviderId, cancellationToken);

        if (provider == null)
        {
            throw new KeyNotFoundException($"Provider profile with ID '{request.ProviderId}' was not found.");
        }

        if (!provider.IsVerified)
        {
            throw new InvalidOperationException("Unverified providers are restricted from creating project templates. Please complete account verification first.");
        }

        // 2. Instantiate the ProjectTemplate domain entity (Initial state defaults to Draft)
        var template = new ProjectTemplate(
            request.ProviderId,
            request.Title,
            request.Description,
            request.ExpectedDurationWeeks);

        // 3. Automatically transition the template from Draft -> PendingReview to match our sprint workflow
        template.SubmitForReview();

        // 4. Persist the template record first to generate the relational context
        context.ProjectTemplates.Add(template);

        // 5. Build and attach the explicit many-to-many join tracking records for required skills
        if (request.SkillIds != null && request.SkillIds.Count != 0)
        {
            // Verify that the requested skills actually exist in our lookups system
            var existingSkillIds = await context.Skills
                .Where(s => request.SkillIds.Contains(s.Id))
                .Select(s => s.Id)
                .ToListAsync(cancellationToken);

            foreach (var skillId in existingSkillIds)
            {
                var templateSkill = new ProjectTemplateSkill(template.Id, skillId);
                context.ProjectTemplateSkills.Add(templateSkill);
            }
        }

        // 6. Save all additions atomically
        await context.SaveChangesAsync(cancellationToken);

        // 7. Return the tracking ID
        return template.Id;
    }
}