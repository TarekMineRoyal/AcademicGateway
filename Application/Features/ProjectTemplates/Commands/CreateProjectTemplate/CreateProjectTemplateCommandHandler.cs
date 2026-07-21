using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.ProjectTemplates;
using AcademicGateway.Domain.ProjectTemplates.Exceptions;
using AcademicGateway.Domain.Providers.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProjectTemplates.Commands.CreateProjectTemplate;

/// <summary>
/// Orchestrates the business command pipeline for instantiating, validating, and persisting a new <see cref="ProjectTemplate"/> blueprint.
/// </summary>
public class CreateProjectTemplateCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<CreateProjectTemplateCommand, Guid>
{
    /// <summary>
    /// Processes the template creation command request, enforcing verification boundaries and tracking technical skill matrices securely.
    /// </summary>
    public async Task<Guid> Handle(CreateProjectTemplateCommand request, CancellationToken cancellationToken)
    {
        // 1. Enforce active security session validation early before executing database logic
        if (!currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Access Denied: Authentication is mandatory to generate project template blueprints.");
        }

        // 2. Retrieve the requesting Provider context
        var provider = await context.Providers
            .FirstOrDefaultAsync(p => p.Id == request.ProviderId, cancellationToken);

        // 3. Validate session boundary and prevent side-channel resource enumeration
        // If the profile is completely missing OR does not align with the logged-in user session,
        // throw a uniform error to fully obscure system data presence from scanning behavior.
        if (provider == null || request.ProviderId != currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("Access Denied: The requested provider profile was not found, or you do not possess blueprint creation authorization permissions.");
        }

        // 4. Enforce the platform verification rule using our strongly-typed domain exception
        if (!provider.IsVerified)
        {
            throw new ProviderNotVerifiedException(request.ProviderId);
        }

        // 5. Instantiate the ProjectTemplate domain entity using our updated constructor sequence (Title, Description, ProviderId, CreatedAt, MajorId, SpecialtyId).
        // Passing a deterministic timestamp from our abstracted system clock ensures full test isolation and decouples the domain from side effects.
        var template = new ProjectTemplate(
            request.Title,
            request.Description,
            request.ProviderId,
            dateTimeProvider.UtcNow,
            request.MajorId,
            request.SpecialtyId);

        // 6. Advance the template lifecycle out of initial draft status to match current workflow requirements
        template.SubmitForReview();

        // 7. Attach technical capability requirements using pure aggregate root behavioral methods.
        if (request.SkillIds != null)
        {
            foreach (var skillId in request.SkillIds)
            {
                // Delegate state modifications entirely to the aggregate root method.
                // This shields application logic from managing direct join-table manipulation mechanics.
                template.AddSkill(skillId);
            }
        }

        // 8. Queue the tracked aggregate root structure for relational database persistence
        context.ProjectTemplates.Add(template);

        // 9. Atomically save additions across all modified tracking tables
        await context.SaveChangesAsync(cancellationToken);

        // 10. Return the assigned surrogate tracking key identifier
        return template.Id;
    }
}