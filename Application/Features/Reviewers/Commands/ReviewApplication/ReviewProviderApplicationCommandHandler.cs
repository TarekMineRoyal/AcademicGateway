using AcademicGateway.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.Reviewers.Commands.ReviewApplication;

/// <summary>
/// Orchestrates the state transition workflow of an external provider onboarding application.
/// Processes administrative decisions and commits domain events atomically via aggregate root behavior.
/// </summary>
public class ReviewProviderApplicationCommandHandler(
    IApplicationDbContext context,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<ReviewProviderApplicationCommand>
{
    /// <summary>
    /// Processes the review decision, applies aggregate transitions, and triggers background synchronization hooks.
    /// </summary>
    /// <param name="request">The structural parameter bundle tracking application evaluation inputs and signatures.</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be canceled.</param>
    /// <returns>A completed asynchronous execution task.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if either the target provider application or the reviewer profile cannot be found.</exception>
    public async Task Handle(ReviewProviderApplicationCommand request, CancellationToken cancellationToken)
    {
        // 1. Fetch the application target from the tracking context
        var application = await context.ProviderApplications
            .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken);

        if (application == null)
        {
            throw new KeyNotFoundException($"Provider application with ID '{request.ApplicationId}' was not found.");
        }

        // 2. Fetch the reviewer profile using the 1:1 primary key strategy mapping directly to the Identity User ID
        // Fixed: Updated lookup target expression to consume 'request.ReviewerId' to align with the refactored command contract.
        var reviewer = await context.Reviewers
            .FirstOrDefaultAsync(r => r.Id == request.ReviewerId, cancellationToken);

        if (reviewer == null)
        {
            throw new KeyNotFoundException($"Reviewer domain profile with ID '{request.ReviewerId}' was not found within the audit directory.");
        }

        // 3. Execute domain state updates using pure behavioral methods
        // Clock timestamps are provided by the injected time provider abstraction to maintain clean, deterministic validation bounds.
        if (request.IsApproved)
        {
            // This transition automatically appends a ProviderApplicationApprovedEvent onto the aggregate outbox
            application.Approve(reviewer.Id, dateTimeProvider.UtcNow);
        }
        else
        {
            application.Reject(reviewer.Id, request.RejectionReason ?? "No reason provided.", dateTimeProvider.UtcNow);
        }

        // 4. Commit all updates. The overridden SaveChangesAsync in ApplicationDbContext intercepts 
        // the application's outbox events, executes the Provider verification logic, and stores 
        // both operations atomically inside a single transaction.
        await context.SaveChangesAsync(cancellationToken);
    }
}