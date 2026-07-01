using AcademicGateway.Application.Common.Interfaces;
using Domain.ProjectTemplates;
using Domain.Providers.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProjectTemplates.Commands.CreateTemplate;

/// <summary>
/// Orchestrates the business command pipeline for instantiating, validating, and persisting a new <see cref="ProjectTemplate"/> blueprint.
/// </summary>
public class CreateProjectTemplateCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateProjectTemplateCommand, Guid>
{
    /// <summary>
    /// Processes the template creation command request, enforcing verification boundaries and tracking technical skill matrices.
    /// </summary>
    /// <param name="request">The incoming command container housing metadata descriptions and required validation arguments.</param>
    /// <param name="cancellationToken">The operational signal tracking asynchronous processing cancellations.</param>
    /// <returns>A unique tracking identifier primary key assigned onto the newly committed project template.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the specified provider identification reference is missing from database records.</exception>
    /// <exception cref="ProviderNotVerifiedException">Thrown if the requesting provider has not successfully completed onboarding verification gates.</exception>
    public async Task<Guid> Handle(CreateProjectTemplateCommand request, CancellationToken cancellationToken)
    {
        // 1. Retrieve the requesting Provider and enforce profile existence
        var provider = await context.Providers
            .FirstOrDefaultAsync(p => p.Id == request.ProviderId, cancellationToken);

        if (provider == null)
        {
            throw new KeyNotFoundException($"Provider profile with ID '{request.ProviderId}' was not found.");
        }

        // 2. Enforce the platform verification rule using our new strongly-typed domain exception
        if (!provider.IsVerified)
        {
            throw new ProviderNotVerifiedException(request.ProviderId);
        }

        // 3. Instantiate the ProjectTemplate domain entity using our updated constructor ordering (Title, Description, ProviderId)
        var template = new ProjectTemplate(
            request.Title,
            request.Description,
            request.ProviderId);

        // 4. Advance the template lifecycle out of initial draft status to match current workflow requirements
        template.SubmitForReview();

        // 5. Verify and attach technical capability requirements using pure aggregate root behavioral methods
        if (request.SkillIds != null && request.SkillIds.Count != 0)
        {
            // Verify that the requested skills actually exist in our lookup database system
            var existingSkillIds = await context.Skills
                .Where(s => request.SkillIds.Contains(s.Id))
                .Select(s => s.Id)
                .ToListAsync(cancellationToken);

            foreach (var skillId in existingSkillIds)
            {
                // DDD Winning Practice: Delegate state modifications entirely to the aggregate root method.
                // This shields your application code from managing direct join-table manipulations.
                template.AddSkill(skillId);
            }
        }

        // 6. Queue the tracked aggregate root structure for relational database persistence
        context.ProjectTemplates.Add(template);

        // 7. Atomically save additions across all modified tracking tables
        await context.SaveChangesAsync(cancellationToken);

        // 8. Return the assigned surrogate tracking key identifier
        return template.Id;
    }
}