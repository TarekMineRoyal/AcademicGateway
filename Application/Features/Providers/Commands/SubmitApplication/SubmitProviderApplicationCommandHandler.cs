using AcademicGateway.Application.Common.Interfaces;
using Domain.Providers;
using Domain.Providers.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.Providers.Commands.SubmitApplication;

public class SubmitProviderApplicationCommandHandler(IApplicationDbContext context)
    : IRequestHandler<SubmitProviderApplicationCommand, Guid>
{
    public async Task<Guid> Handle(SubmitProviderApplicationCommand request, CancellationToken cancellationToken)
    {
        // 1. Parse and validate the incoming ProviderId string into a Guid matching the domain entity key type
        if (!Guid.TryParse(request.ProviderId, out var providerGuid))
        {
            throw new ArgumentException("The provided Provider ID is not in a valid Guid format.", nameof(request.ProviderId));
        }

        // 2. Guard: Ensure the requesting Provider profile actually exists in the system
        var providerExists = await context.Providers
            .AnyAsync(p => p.Id == providerGuid, cancellationToken);

        if (!providerExists)
        {
            throw new KeyNotFoundException($"Provider profile with ID '{request.ProviderId}' was not found.");
        }

        // 3. Retrieve any existing onboarding application for this provider to fulfill the strict 1-to-1 lifecycle requirement
        var existingApplication = await context.ProviderApplications
            .FirstOrDefaultAsync(a => a.ProviderId == providerGuid, cancellationToken);

        if (existingApplication != null)
        {
            // Guard: If it's already Approved, block subsequent modification attempts
            if (existingApplication.Status == ProviderApplicationStatus.Approved)
            {
                throw new InvalidOperationException($"Provider '{request.ProviderId}' is already formally approved and verified.");
            }

            // Guard: If it's currently Pending Review, prevent double submissions
            if (existingApplication.Status == ProviderApplicationStatus.PendingReview)
            {
                throw new InvalidOperationException($"Provider '{request.ProviderId}' already has an onboarding application actively pending review.");
            }

            // State-Machine Loop: If the previous submission was Rejected, invoke the stateful Resubmit domain method
            if (existingApplication.Status == ProviderApplicationStatus.Rejected)
            {
                existingApplication.Resubmit(request.CompanyDetails, request.VerificationDocumentsUrl);
            }
            else
            {
                // Fallback safety catch for edge-case lifecycles (e.g., Draft states)
                throw new InvalidOperationException($"Cannot process submission. The current application state is: {existingApplication.Status}");
            }

            await context.SaveChangesAsync(cancellationToken);
            return existingApplication.Id;
        }

        // 4. Process a brand-new application sequence if no prior record exists
        var newApplication = new ProviderApplication(
            providerGuid,
            request.CompanyDetails,
            request.VerificationDocumentsUrl,
            DateTime.UtcNow); // Clock timestamp is passed explicitly per domain deterministic guidelines

        // Advance state machine from Draft -> PendingReview
        newApplication.SubmitForReview();

        context.ProviderApplications.Add(newApplication);
        await context.SaveChangesAsync(cancellationToken);

        return newApplication.Id;
    }
}