using AcademicGateway.Application.Common.Interfaces;
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
        // 1. Retrieve the incoming application alongside its matching core Provider profile
        var application = await context.ProviderApplications
            .Include(pa => pa.Provider)
            .FirstOrDefaultAsync(pa => pa.Id == request.ApplicationId, cancellationToken);

        if (application == null)
        {
            throw new KeyNotFoundException($"Provider application with ID '{request.ApplicationId}' was not found.");
        }

        // 2. Retrieve the Reviewer entity associated with the authenticated Identity User
        var reviewer = await context.Reviewers
            .FirstOrDefaultAsync(r => r.IdentityUserId == request.ReviewerIdentityUserId, cancellationToken);

        if (reviewer == null)
        {
            throw new KeyNotFoundException($"Reviewer domain profile for Identity User ID '{request.ReviewerIdentityUserId}' was not found.");
        }

        // 3. Apply state machine logic using domain guard clauses
        if (request.IsApproved)
        {
            application.Approve(reviewer.Id);

            // Business Rule: Transition the primary Provider entity status to verified 
            if (application.Provider != null)
            {
                application.Provider.IsVerified = true;
            }
        }
        else
        {
            application.Reject(reviewer.Id, request.RejectionReason ?? "No specific rejection details provided by reviewer.");
        }

        // 4. Save updates to the persistence layer
        await context.SaveChangesAsync(cancellationToken);
    }
}