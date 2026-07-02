using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGateway.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGateway.Application.Features.TechSupportProposals.Commands.ReviewTechSupportProposal;

/// <summary>
/// Orchestrates the application logic for processing a student's review over an incoming corporate mentorship assignment.
/// </summary>
public class ReviewTechSupportProposalCommandHandler : IRequestHandler<ReviewTechSupportProposalCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReviewTechSupportProposalCommandHandler"/> class.
    /// </summary>
    /// <param name="context">The unit-of-work relational data access boundary layer.</param>
    /// <param name="currentUserService">Provides tracking visibility over the currently authenticated session security credentials.</param>
    public ReviewTechSupportProposalCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Validates student ownership permissions, executes the evaluation transition inside the aggregate root, and saves changes.
    /// </summary>
    /// <param name="request">The incoming CQRS data payload detailing the student's evaluation decision.</param>
    /// <param name="cancellationToken">The operational signal tracking asynchronous execution cancellations.</param>
    /// <returns>A transactional execution confirmation wrapper unit.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if either the target workspace or specific corporate proposal entry cannot be located.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if an unauthorized user attempts to act on the proposal.</exception>
    public async Task<Unit> Handle(ReviewTechSupportProposalCommand request, CancellationToken cancellationToken)
    {
        // 1. Retrieve the target project instance along with its nested child proposal collections
        var projectInstance = await _context.ProjectInstances
            .Include(pi => pi.TechSupportProposals)
            .FirstOrDefaultAsync(pi => pi.Id == request.ProjectInstanceId, cancellationToken);

        if (projectInstance == null)
        {
            throw new KeyNotFoundException($"The target project instance workspace with ID '{request.ProjectInstanceId}' was not found.");
        }

        // 2. Security Boundaries: Enforce that only the student owner who initialized the workspace can review corporate offers
        if (!_currentUserService.IsAuthenticated || projectInstance.StudentId != _currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("Access Denied: You are not authorized to evaluate corporate assistance offers for this workspace.");
        }

        // 3. Locate the targeted individual corporate proposal child entity within our aggregate scope
        var proposal = projectInstance.TechSupportProposals
            .FirstOrDefault(p => p.Id == request.TechSupportProposalId);

        if (proposal == null)
        {
            throw new KeyNotFoundException($"Corporate mentor proposal with tracking key '{request.TechSupportProposalId}' was not found in this workspace context.");
        }

        // 4. Delegate core business invariant processing and state alterations downstream to the Aggregate Root
        projectInstance.ReviewTechSupportProposal(request.TechSupportProposalId, request.Accept, request.RejectionReason);

        // 5. Commit state mutations down to physical database layers atomically
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}