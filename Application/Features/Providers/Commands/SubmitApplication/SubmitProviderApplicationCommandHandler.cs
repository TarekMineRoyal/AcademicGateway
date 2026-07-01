using AcademicGateway.Application.Common.Interfaces;
using Domain.Providers;
using Domain.Providers.Enums;
using Domain.Providers.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.Providers.Commands.SubmitApplication;

/// <summary>
/// Orchestrates the business command pipeline for processing, validating, and transitioning provider onboarding application workflows.
/// Enforces a strict 1-to-1 aggregate root allocation boundary for processing enrollment validation streams.
/// </summary>
public class SubmitProviderApplicationCommandHandler(
    IApplicationDbContext context,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<SubmitProviderApplicationCommand, Guid>
{
    /// <summary>
    /// Processes the application submission command request, managing state transitions and ensuring 1-to-1 lifecycle compliance bounds.
    /// </summary>
    /// <param name="request">The incoming command container housing company background profiles and legal file references.</param>
    /// <param name="cancellationToken">The operational signal tracking asynchronous processing cancellations.</param>
    /// <returns>A unique tracking identifier primary key assigned onto the newly committed or updated application workflow track.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the specified provider identification reference is missing from database records.</exception>
    /// <exception cref="InvalidApplicationStatusException">Thrown if an application exists in an immutable or non-resubmittable lifecycle state.</exception>
    public async Task<Guid> Handle(SubmitProviderApplicationCommand request, CancellationToken cancellationToken)
    {
        // 1. Guard: Ensure the requesting Provider profile actually exists in the system using the clean, strongly-typed Guid key
        var providerExists = await context.Providers
            .AsNoTracking()
            .AnyAsync(p => p.Id == request.ProviderId, cancellationToken);

        if (!providerExists)
        {
            throw new KeyNotFoundException($"Provider profile with ID '{request.ProviderId}' was not found.");
        }

        // 2. Retrieve any existing onboarding application for this provider to fulfill the strict 1-to-1 lifecycle requirement
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

            // State-Machine Loop: If the previous submission was Rejected, invoke the stateful Resubmit domain method
            if (existingApplication.Status == ProviderApplicationStatus.Rejected)
            {
                existingApplication.Resubmit(request.CompanyDetails, request.VerificationDocumentsUrl);
            }
            else
            {
                // Fallback safety catch for unhandled edge-case states (e.g., Draft states if introduced later)
                throw new InvalidApplicationStatusException(existingApplication.Status, "SubmitNewApplication");
            }

            await context.SaveChangesAsync(cancellationToken);
            return existingApplication.Id;
        }

        // 3. Process a brand-new application sequence if no prior record exists
        // Clock timestamp is passed explicitly per domain deterministic guidelines via the injected time provider abstraction
        var newApplication = new ProviderApplication(
            request.ProviderId,
            request.CompanyDetails,
            request.VerificationDocumentsUrl,
            dateTimeProvider.UtcNow);

        // Advance state machine out of internal draft status straight into the institutional pool
        newApplication.SubmitForReview();

        context.ProviderApplications.Add(newApplication);
        await context.SaveChangesAsync(cancellationToken);

        return newApplication.Id;
    }
}