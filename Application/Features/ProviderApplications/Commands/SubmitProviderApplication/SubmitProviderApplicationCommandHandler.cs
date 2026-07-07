using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.Providers;
using AcademicGateway.Domain.Providers.Enums;
using AcademicGateway.Domain.Providers.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProviderApplications.Commands.SubmitProviderApplication;

/// <summary>
/// Orchestrates the business command pipeline for processing, validating, and transitioning provider onboarding application workflows.
/// Fortified against Broken Object Level Authorization (BOLA) and side-channel resource enumeration vectors.
/// </summary>
public class SubmitProviderApplicationCommandHandler(
    IApplicationDbContext context,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService)
    : IRequestHandler<SubmitProviderApplicationCommand, Guid>
{
    /// <summary>
    /// Processes the application submission command request, managing state transitions and ensuring 1-to-1 lifecycle compliance bounds securely.
    /// </summary>
    public async Task<Guid> Handle(SubmitProviderApplicationCommand request, CancellationToken cancellationToken)
    {
        // 1. Enforce active security session validation early before executing database logic
        if (!currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Access Denied: Authentication is mandatory to submit corporate onboarding applications.");
        }

        // 2. Guard: Verify that the profile exists in the system directory context
        var providerExists = await context.Providers
            .AsNoTracking()
            .AnyAsync(p => p.Id == request.ProviderId, cancellationToken);

        // 3. Validate session boundary and prevent side-channel resource enumeration
        // If the profile is completely missing OR does not align with the logged-in user session,
        // throw a uniform error to fully obscure system data presence from scanning behavior.
        if (!providerExists || request.ProviderId != currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("Access Denied: The requested provider profile was not found, or you do not possess application submission authorization permissions.");
        }

        // 4. Retrieve any existing onboarding application for this provider to fulfill the strict 1-to-1 lifecycle requirement
        var existingApplication = await context.ProviderApplications
            .FirstOrDefaultAsync(a => a.ProviderId == request.ProviderId, cancellationToken);

        if (existingApplication != null)
        {
            // State-Machine Guards: Prevent double-submitting if an audit cycle is already active or completed
            if (existingApplication.Status == ProviderApplicationStatus.Approved ||
                existingApplication.Status == ProviderApplicationStatus.PendingReview)
            {
                throw new InvalidApplicationStatusException(existingApplication.Status, "SubmitNewApplication");
            }

            // State-Machine Loop: If the previous submission was Rejected, invoke the resubmit domain logic
            if (existingApplication.Status == ProviderApplicationStatus.Rejected)
            {
                existingApplication.Resubmit(request.CompanyDetails, request.VerificationDocumentsUrl);
            }
            else
            {
                // Fallback safety catch for unhandled edge-case states
                throw new InvalidApplicationStatusException(existingApplication.Status, "SubmitNewApplication");
            }

            await context.SaveChangesAsync(cancellationToken);
            return existingApplication.Id;
        }

        // 5. Process a brand-new application sequence if no prior record exists
        var newApplication = new ProviderApplication(
            request.ProviderId,
            request.CompanyDetails,
            request.VerificationDocumentsUrl,
            dateTimeProvider.UtcNow);

        // Advance state machine out of internal draft status straight into the institutional review pool
        newApplication.SubmitForReview();

        context.ProviderApplications.Add(newApplication);
        await context.SaveChangesAsync(cancellationToken);

        return newApplication.Id;
    }
}