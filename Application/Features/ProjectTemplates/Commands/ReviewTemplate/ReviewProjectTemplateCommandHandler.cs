using AcademicGateway.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProjectTemplates.Commands.ReviewTemplate;

public class ReviewProjectTemplateCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ReviewProjectTemplateCommand>
{
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
        var reviewer = await context.Reviewers
            .FirstOrDefaultAsync(r => r.IdentityUserId == request.ReviewerIdentityUserId, cancellationToken);

        if (reviewer == null)
        {
            throw new KeyNotFoundException($"Reviewer domain profile for Identity User ID '{request.ReviewerIdentityUserId}' was not found.");
        }

        // 3. Invoke the domain-level state machine guard clauses
        if (request.IsApproved)
        {
            template.Approve(reviewer.Id);
        }
        else
        {
            template.Reject(reviewer.Id, request.RejectionReason ?? "No specific rejection details provided by reviewer.");
        }

        // 4. Save the workflow updates to PostgreSQL
        await context.SaveChangesAsync(cancellationToken);
    }
}