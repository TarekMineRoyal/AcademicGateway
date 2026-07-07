using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.ProjectTemplates.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProjectTemplates.Commands.ReviewProjectTemplate;

/// <summary>
/// Handles the transaction routine to process a <see cref="ReviewProjectTemplateCommand"/>.
/// Locates the template aggregate, verifies evaluator profile presence, and invokes internal state machine rules securely.
/// </summary>
public class ReviewProjectTemplateCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<ReviewProjectTemplateCommand>
{
    /// <summary>
    /// Processes the audit decision, executes the corresponding domain state transition, and flushes changes to storage securely.
    /// </summary>
    /// <param name="request">The structural parameter bundle tracking evaluation state results and reviewer signatures.</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be canceled.</param>
    /// <returns>A completed asynchronous execution task.</returns>
    /// <exception cref="UnauthorizedAccessException">Uniformly thrown if session authentication fails, resources don't exist, or tenancy fails validation.</exception>
    public async Task Handle(ReviewProjectTemplateCommand request, CancellationToken cancellationToken)
    {
        // Enforce active security session validation early before executing database logic
        if (!currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Access Denied: Authentication is mandatory to execute template audits.");
        }

        // Retrieve the incoming project template from the database
        var template = await context.ProjectTemplates
            .FirstOrDefaultAsync(pt => pt.Id == request.TemplateId, cancellationToken);

        // Retrieve the Reviewer domain profile linked to the target context
        var reviewerExists = await context.Reviewers
            .AnyAsync(r => r.Id == request.ReviewerId, cancellationToken);

        // Validate presence boundaries and verify that the session user ID matches the target Reviewer identity.
        // Using a single unified error boundary protects against side-channel resource enumeration vectors.
        if (template == null || !reviewerExists || request.ReviewerId != currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("Access Denied: The requested resource was not found, or you do not possess audit evaluation authorization permissions.");
        }

        // Invoke explicit domain-level state machine boundaries
        if (request.IsApproved)
        {
            template.Approve();
        }
        else
        {
            template.RejectPermanently(request.RejectionReason ?? "No specific rejection details provided by reviewer.");
        }

        // Save the workflow updates securely
        await context.SaveChangesAsync(cancellationToken);
    }
}