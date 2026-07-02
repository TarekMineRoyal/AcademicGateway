using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.ProjectInstances.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGateway.Application.Features.TechSupportProposals.Commands.ProposeTechSupport;

/// <summary>
/// Orchestrates the application logic for a corporate provider to assign a technical mentor to a running student workspace.
/// </summary>
public class ProposeTechSupportCommandHandler : IRequestHandler<ProposeTechSupportCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProposeTechSupportCommandHandler"/> class.
    /// </summary>
    /// <param name="context">The unit-of-work relational data access boundary layer.</param>
    /// <param name="currentUserService">Provides tracking visibility over the currently authenticated session security credentials.</param>
    /// <param name="dateTimeProvider">The deterministic system clock wrapper abstraction.</param>
    public ProposeTechSupportCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    /// <summary>
    /// Validates corporate tenancy boundaries, verifies support account existence, and tracks the mentorship proposal.
    /// </summary>
    /// <param name="request">The incoming CQRS data payload containing workspace routing identifiers and assignment keys.</param>
    /// <param name="cancellationToken">The operational signal tracking asynchronous execution cancellations.</param>
    /// <returns>The generated primary tracking key Guid of the newly appended proposal entity log.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if either the target project workspace or support account profile does not exist.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if an unauthenticated user or an unrelated corporate entity attempts the assignment.</exception>
    public async Task<Guid> Handle(ProposeTechSupportCommand request, CancellationToken cancellationToken)
    {
        // 1. Retrieve the target project instance workspace along with its active corporate engagement tracking loops
        var projectInstance = await _context.ProjectInstances
            .Include(pi => pi.TechSupportProposals)
            .FirstOrDefaultAsync(pi => pi.Id == request.ProjectInstanceId, cancellationToken);

        if (projectInstance == null)
        {
            throw new KeyNotFoundException($"The target project instance workspace with ID '{request.ProjectInstanceId}' was not found.");
        }

        // 2. Security Boundaries: Enforce that only the specific corporate provider who owns the original template can pitch mentorship
        if (!_currentUserService.IsAuthenticated || projectInstance.ProviderId != _currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("Access Denied: You are not authorized to assign technical support personnel to this project workspace.");
        }

        // 3. Relational Pre-Check: Verify that the designated corporate tech support engineer account profile exists
        var techSupportAccountExists = await _context.TechSupportAccounts
            .AnyAsync(ts => ts.Id == request.TechSupportAccountId, cancellationToken);

        if (!techSupportAccountExists)
        {
            throw new KeyNotFoundException($"The corporate tech support profile with ID '{request.TechSupportAccountId}' does not exist.");
        }

        // 4. Delegate core state transition checks and entity initialization downstream to the Aggregate Root
        projectInstance.ProposeTechSupport(request.TechSupportAccountId, _dateTimeProvider.UtcNow);

        // 5. Commit state modifications down to physical relational rows atomically
        await _context.SaveChangesAsync(cancellationToken);

        // 6. Extract the tracking identifier of the newly appended proposal within our domain boundary collection
        var newlyCreatedProposal = projectInstance.TechSupportProposals
            .First(p => p.TechSupportAccountId == request.TechSupportAccountId && p.Status == TechSupportProposalStatus.Pending);

        return newlyCreatedProposal.Id;
    }
}