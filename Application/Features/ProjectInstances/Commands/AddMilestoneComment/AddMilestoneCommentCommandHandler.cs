using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.ProjectInstances.Exceptions;
using Application.Features.ProjectInstances.Commands.AddMilestoneComment;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProjectInstances.Commands.AddMilestoneComment;

/// <summary>
/// Orchestrates the application logic for appending a cross-role conversation comment onto a milestone lane.
/// Fetches the target workspace aggregate root, routes parameters safely, and persists the conversation row.
/// </summary>
public class AddMilestoneCommentCommandHandler : IRequestHandler<AddMilestoneCommentCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddMilestoneCommentCommandHandler"/> class.
    /// </summary>
    /// <param name="context">The application data persistence context boundary interface.</param>
    /// <param name="dateTimeProvider">The deterministic system clock abstraction layer provider.</param>
    public AddMilestoneCommentCommandHandler(IApplicationDbContext context, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
    }

    /// <summary>
    /// Processes the message commentary append request, ensuring aggregate encapsulation parameters are respected.
    /// </summary>
    /// <param name="request">The incoming command model carrying the author token, message copy, and location coordinates.</param>
    /// <param name="cancellationToken">The asynchronous operation cancellation tracking token.</param>
    /// <returns>A MediatR completion compliance unit instance.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the project workspace aggregate root cannot be resolved.</exception>
    /// <exception cref="InvalidProjectInstanceTransitionException">Thrown if tracking lifecycle locks block conversational text postings.</exception>
    public async Task<Unit> Handle(AddMilestoneCommentCommand request, CancellationToken cancellationToken)
    {
        // Architectural Necessity: To allow the aggregate root boundary to pass the comment parameters 
        // down to its internal target milestone item, we load the instance workspace with its LocalMilestones collection.
        var projectInstance = await _context.ProjectInstances
            .Include(p => p.LocalMilestones)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectInstanceId, cancellationToken);

        if (projectInstance == null)
        {
            throw new KeyNotFoundException($"Project Instance workspace with ID '{request.ProjectInstanceId}' was not found.");
        }

        // Pass parameters down onto the aggregate root to maintain pure DDD domain orchestration loops
        projectInstance.AddMilestoneComment(
            request.LocalMilestoneId,
            request.AuthorId,
            request.AuthorIdentitySnapshot,
            request.Content,
            _dateTimeProvider.UtcNow);

        // Commit the new conversation entry records down to persistent storage fields atomically
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}