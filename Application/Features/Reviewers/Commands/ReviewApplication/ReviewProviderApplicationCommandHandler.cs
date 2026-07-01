using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.Enums;
using Domain.Providers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.Reviewers.Commands.ReviewApplication;

public class ReviewProviderApplicationCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ReviewProviderApplicationCommand>
{
    public async Task Handle(ReviewProviderApplicationCommand request, CancellationToken cancellationToken)
    {
        // 1. Fetch the application target
        var application = await context.ProviderApplications
            .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken);

        if (application == null)
        {
            throw new KeyNotFoundException($"Provider application with ID '{request.ApplicationId}' was not found.");
        }

        // 2. Fetch the reviewer profile executing the command
        var reviewer = await context.Reviewers
            .FirstOrDefaultAsync(r => r.IdentityUserId == request.ReviewerIdentityUserId, cancellationToken);

        if (reviewer == null)
        {
            throw new KeyNotFoundException($"Reviewer domain profile for Identity User ID '{request.ReviewerIdentityUserId}' was not found.");
        }

        // 3. Execute state transition updates
        if (request.IsApproved)
        {
            application.Approve(reviewer.Id);

            // CASCADING FIX: Fetch the underlying provider and flip their verification flag to true
            var provider = await context.Providers
                .FirstOrDefaultAsync(p => p.UserId == application.ProviderId, cancellationToken);

            if (provider != null)
            {
                // If your Provider entity uses a method like provider.Verify(), invoke that here instead!
                typeof(Provider)
                    .GetProperty(nameof(Provider.IsVerified))?
                    .SetValue(provider, true);
            }
        }
        else
        {
            application.Reject(reviewer.Id, request.RejectionReason ?? "No reason provided.");
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}