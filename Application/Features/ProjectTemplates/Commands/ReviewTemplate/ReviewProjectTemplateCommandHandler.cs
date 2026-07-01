using AcademicGateway.Application.Common.Interfaces;
using Domain.ProjectTemplates.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProjectTemplates.Commands.ReviewTemplate;

/// <summary>
/// Handles the transaction routine to process a <see cref="ReviewProjectTemplateCommand"/>.
/// Locates the template aggregate, verifies evaluator profile presence, and invokes internal state machine rules.
/// </summary>
public class ReviewProjectTemplateCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ReviewProjectTemplateCommand>
{
    /// <summary>
    /// Processes the audit decision, executes the corresponding domain state transition, and flushes changes to storage.
    /// </summary>
    /// <param name="request">The structural parameter bundle tracking evaluation state results and reviewer signatures.</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be canceled.</param>
    /// <returns>A completed asynchronous execution task.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if either the target project blueprint or the reviewer profile cannot be found.</exception>
    /// <exception cref="InvalidTemplateStatusException">Thrown if executing the evaluation transition violates lifecycle order rules.</exception>
    /// <exception cref="InvalidTemplateDetailsException">Thrown if provided feedback or rejection reasoning parameters break content invariants.</exception>
    public async Task Handle(ReviewProjectTemplateCommand request, CancellationToken cancellationToken)
    {
        // 1. Retrieve the incoming project template from the database
        var template = await context.ProjectTemplates
            .FirstOrDefaultAsync(pt => pt.Id == request.TemplateId, cancellationToken);

        if (template == null)
        {
            throw new KeyNotFoundException($"Project template with ID '{request.TemplateId}' was not found.");
        }

        // 2. Retrieve the Reviewer domain profile linked to the current identity session
        // Architectural Alignment: Reviewer.Id directly stores and maps 1:1 to the identity user account context.
        var reviewerExists = await context.Reviewers
            .AnyAsync(r => r.Id == request.ReviewerId, cancellationToken);

        if (!reviewerExists)
        {
            throw new KeyNotFoundException($"Reviewer domain profile with ID '{request.ReviewerId}' was not found within the audit directory.");
        }

        // 3. Invoke explicit domain-level state machine boundaries
        // This ensures tracking histories are sealed cleanly and private states remain fully encapsulated.
        if (request.IsApproved)
        {
            template.Approve();
        }
        else
        {
            // Maps to your domain's formal denial lifecycle sequence rule
            template.RejectPermanently(request.RejectionReason ?? "No specific rejection details provided by reviewer.");
        }

        // 4. Save the workflow updates to PostgreSQL
        await context.SaveChangesAsync(cancellationToken);
    }
}