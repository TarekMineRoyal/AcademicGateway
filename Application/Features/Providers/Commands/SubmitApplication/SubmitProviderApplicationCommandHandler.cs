using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.Entities;
using AcademicGateway.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.Providers.Commands.SubmitApplication;

public class SubmitProviderApplicationCommandHandler(IApplicationDbContext context)
    : IRequestHandler<SubmitProviderApplicationCommand, Guid>
{
    public async Task<Guid> Handle(SubmitProviderApplicationCommand request, CancellationToken cancellationToken)
    {
        // 1. Guard: Ensure the requesting Provider profile actually exists
        var providerExists = await context.Providers
            .AnyAsync(p => p.UserId == request.ProviderId, cancellationToken);

        if (!providerExists)
        {
            throw new KeyNotFoundException($"Provider profile with User ID '{request.ProviderId}' was not found.");
        }

        // 2. Guard: Prevent duplicate submissions if an application is already active/pending
        var hasActiveApplication = await context.ProviderApplications
            .AnyAsync(a => a.ProviderId == request.ProviderId &&
                           (a.Status == ProviderApplicationStatus.PendingReview || a.Status == ProviderApplicationStatus.Approved),
                      cancellationToken);

        if (hasActiveApplication)
        {
            throw new InvalidOperationException($"Provider '{request.ProviderId}' already has an active onboarding application.");
        }

        // 3. Process insertion
        var application = new ProviderApplication(
            request.ProviderId,
            request.CompanyDetails,
            request.VerificationDocumentsUrl);

        context.ProviderApplications.Add(application);
        await context.SaveChangesAsync(cancellationToken);

        return application.Id;
    }
}