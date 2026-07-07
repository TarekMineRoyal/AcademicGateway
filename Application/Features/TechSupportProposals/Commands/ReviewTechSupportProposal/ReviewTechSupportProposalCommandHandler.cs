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
    /// Validates student ownership permissions, executes the evaluation transition inside the aggregate root, and saves changes securely.
    /// </summary>
    /// <param name="request">The incoming CQRS data payload detailing the student's evaluation decision.</param>
    /// <param name="cancellationToken">The operational signal tracking asynchronous execution cancellations.</param>
    /// <returns>A transactional execution confirmation wrapper unit.</returns>
    /// <exception cref="UnauthorizedAccessException">Uniformly thrown if authentication is missing, resources don't exist, or tenancy parameters break invariants.</exception>
    public async Task<Unit> Handle(ReviewTechSupportProposalCommand request, CancellationToken cancellationToken)
    {
        // 1. Enforce active security session validation early before executing database queries
        if (!_currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Access Denied: Authentication is mandatory to review technical support proposals.");
        }

        // 2. Retrieve the target project instance along with its nested child proposal collections
        var projectInstance = await _context.ProjectInstances
            .Include(pi => pi.TechSupportProposals)
            .FirstOrDefaultAsync(pi => pi.Id == request.ProjectInstanceId, cancellationToken);

        // 3. Locate the targeted individual corporate proposal child entity within our aggregate scope using defensive navigation mapping
        var proposal = projectInstance?.TechSupportProposals
            .FirstOrDefault(p => p.Id == request.TechSupportProposalId);

        // 4. Protect against side-channel resource enumeration
        // Coalesce parent aggregate null checks, child entity null checks, and student user owner tenancy checks.
        // Uniformly throw an UnauthorizedAccessException to completely obscure whether the ID is missing or unowned.
        if (projectInstance == null || proposal == null || projectInstance.StudentId != _currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("Access Denied: The requested proposal record was not found, or you do not possess evaluation authorization permissions.");
        }

        // 5. Delegate core business invariant processing and state alterations downstream to the Aggregate Root
        projectInstance.ReviewTechSupportProposal(request.TechSupportProposalId, request.Accept, request.RejectionReason);

        // 6. Commit state mutations down to physical database layers atomically
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}