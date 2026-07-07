using AcademicGateway.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProviderApplications.Commands.ReviewProviderApplication;

/// <summary>
/// Orchestrates the state transition workflow of an external provider onboarding application.
/// Fortified against Broken Object Level Authorization (BOLA) and side-channel resource enumeration vectors.
/// </summary>
public class ReviewProviderApplicationCommandHandler(
    IApplicationDbContext context,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService)
    : IRequestHandler<ReviewProviderApplicationCommand>
{
    /// <summary>
    /// Processes the review decision, applies aggregate transitions, and triggers verification hooks securely.
    /// </summary>
    /// <param name="request">The structural parameter bundle tracking application evaluation inputs and signatures.</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be canceled.</param>
    /// <returns>A completed asynchronous execution task.</returns>
    /// <exception cref="UnauthorizedAccessException">Uniformly thrown if session authentication fails, resources don't exist, or tenancy fails validation.</exception>
    public async Task Handle(ReviewProviderApplicationCommand request, CancellationToken cancellationToken)
    {
        // 1. Enforce active security session validation early before executing database logic
        if (!currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Access Denied: Authentication is mandatory to execute application audits.");
        }

        // 2. Fetch the application target from the tracking context
        var application = await context.ProviderApplications
            .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken);

        // 3. Fetch the reviewer profile using the 1:1 primary key strategy mapping directly to the Identity User ID
        var reviewer = await context.Reviewers
            .FirstOrDefaultAsync(r => r.Id == request.ReviewerId, cancellationToken);

        // 4. Verify session tenancy boundaries and eliminate side-channel enumeration
        // If the targeted reviewer profile does not align with the current session token, OR if the 
        // requested application/reviewer records are missing, return an identical non-descriptive error.
        if (application == null || reviewer == null || request.ReviewerId != currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("Access Denied: The requested resource was not found, or you do not possess evaluation review authorization permissions.");
        }

        // 5. Execute domain state updates using pure behavioral methods
        if (request.IsApproved)
        {
            // This transition automatically appends a ProviderApplicationApprovedEvent onto the aggregate outbox
            application.Approve(reviewer.Id, dateTimeProvider.UtcNow);
        }
        else
        {
            application.Reject(reviewer.Id, request.RejectionReason ?? "No reason provided.", dateTimeProvider.UtcNow);
        }

        // 6. Commit all updates atomically inside a single transaction
        await context.SaveChangesAsync(cancellationToken);
    }
}