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
    /// Validates corporate tenancy boundaries, verifies support account existence, and tracks the mentorship proposal securely.
    /// </summary>
    /// <param name="request">The incoming CQRS data payload containing workspace routing identifiers and assignment keys.</param>
    /// <param name="cancellationToken">The operational signal tracking asynchronous execution cancellations.</param>
    /// <returns>The generated primary tracking key Guid of the newly appended proposal entity log.</returns>
    /// <exception cref="UnauthorizedAccessException">Uniformly thrown if session authentication fails or tenancy bounds reject validation.</exception>
    /// <exception cref="KeyNotFoundException">Thrown if the target technical staff profile does not exist.</exception>
    public async Task<Guid> Handle(ProposeTechSupportCommand request, CancellationToken cancellationToken)
    {
        // 1. Enforce active session layout verification early before querying downstream persistence maps
        if (!_currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Access Denied: Authentication is mandatory to assign technical support personnel.");
        }

        // 2. Retrieve the target project instance workspace along with its active corporate engagement tracking loops
        var projectInstance = await _context.ProjectInstances
            .Include(pi => pi.TechSupportProposals)
            .FirstOrDefaultAsync(pi => pi.Id == request.ProjectInstanceId, cancellationToken);

        // 3. Protect against side-channel resource enumeration
        // Uniformly enforce the boundary validation check: If the item is missing OR owned by another provider context,
        // throw an identical UnauthorizedAccessException to ensure the API never leaks structural presence indicators.
        if (projectInstance == null || projectInstance.ProviderId != _currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("Access Denied: The requested project workspace was not found, or you do not possess management authorization permissions.");
        }

        // 4. Relational Pre-Check: Verify that the designated corporate tech support engineer account profile exists
        var techSupportAccountExists = await _context.TechSupportAccounts
            .AnyAsync(ts => ts.Id == request.TechSupportAccountId, cancellationToken);

        if (!techSupportAccountExists)
        {
            throw new KeyNotFoundException($"The corporate tech support profile with ID '{request.TechSupportAccountId}' does not exist within the firm directory.");
        }

        // 5. Delegate core state transition checks and entity initialization downstream to the Aggregate Root
        projectInstance.ProposeTechSupport(request.TechSupportAccountId, _dateTimeProvider.UtcNow);

        // 6. Commit state modifications down to physical relational rows atomically
        await _context.SaveChangesAsync(cancellationToken);

        // 7. Avoid unsafe collection traversal assumptions
        // Substitute the loose `.First()` implementation pattern for a safer `.LastOrDefault()` lookup constraint strategy.
        // This naturally targets the exact tracking record appended during this specific transaction iteration context.
        var newlyCreatedProposal = projectInstance.TechSupportProposals
            .LastOrDefault(p => p.TechSupportAccountId == request.TechSupportAccountId && p.Status == TechSupportProposalStatus.Pending);

        if (newlyCreatedProposal == null)
        {
            throw new InvalidOperationException("An unexpected processing error occurred while materializing the mentorship assignment tracking record.");
        }

        return newlyCreatedProposal.Id;
    }
}