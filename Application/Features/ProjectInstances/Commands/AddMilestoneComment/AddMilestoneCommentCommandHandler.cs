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
/// Fetches the target workspace aggregate root, routes parameters safely, and persists the conversation row securely.
/// </summary>
public class AddMilestoneCommentCommandHandler : IRequestHandler<AddMilestoneCommentCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddMilestoneCommentCommandHandler"/> class.
    /// </summary>
    /// <param name="context">The application data persistence context boundary interface.</param>
    /// <param name="dateTimeProvider">The deterministic system clock abstraction layer provider.</param>
    /// <param name="currentUserService">Provides tracking visibility over the currently authenticated session security credentials.</param>
    public AddMilestoneCommentCommandHandler(
        IApplicationDbContext context,
        IDateTimeProvider dateTimeProvider,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Processes the message commentary append request, ensuring aggregate encapsulation parameters are respected securely.
    /// </summary>
    /// <param name="request">The incoming command model carrying the author token, message copy, and location coordinates.</param>
    /// <param name="cancellationToken">The asynchronous operation cancellation tracking token.</param>
    /// <returns>A MediatR completion compliance unit instance.</returns>
    /// <exception cref="UnauthorizedAccessException">Uniformly thrown if session authentication fails, resources don't exist, or tenancy fails validation.</exception>
    public async Task<Unit> Handle(AddMilestoneCommentCommand request, CancellationToken cancellationToken)
    {
        // Enforce active security session validation early before executing database logic
        if (!_currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Access Denied: Authentication is mandatory to post discussion comments.");
        }

        // Verify identity cross-referencing to prevent author context spoofing
        if (request.AuthorId != _currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("Access Denied: You cannot author a comment on behalf of a different identity profile.");
        }

        // Load the instance workspace along with its required internal collections
        var projectInstance = await _context.ProjectInstances
            .Include(p => p.LocalMilestones)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectInstanceId, cancellationToken);

        // Validate aggregate presence and contextual user tenancy boundaries uniformly.
        // Discussion access is strictly restricted to the participating student, supervisor, or provider profiles.
        if (projectInstance == null || (projectInstance.StudentId != _currentUserService.UserId &&
                                        projectInstance.SupervisorId != _currentUserService.UserId &&
                                        projectInstance.ProviderId != _currentUserService.UserId))
        {
            throw new UnauthorizedAccessException("Access Denied: The requested project workspace was not found, or you do not possess conversation authorization permissions.");
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