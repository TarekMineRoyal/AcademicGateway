using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AcademicGateway.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGateway.Application.Features.ProjectInstances.Commands.ConcludeProject;

/// <summary>
/// Handles the orchestration flow to cleanly conclude an active running project workspace.
/// Fortified against Broken Object Level Authorization (BOLA) and side-channel resource enumeration vectors.
/// </summary>
public class ConcludeProjectCommandHandler : IRequestHandler<ConcludeProjectCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public ConcludeProjectCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(ConcludeProjectCommand request, CancellationToken cancellationToken)
    {
        // 1. Enforce active session presence before interacting with downstream persistent data fields
        if (!_currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Access Denied: Authentication is mandatory to conclude workspace lifecycle operations.");
        }

        var projectInstance = await _context.ProjectInstances
            .FirstOrDefaultAsync(pi => pi.Id == request.ProjectInstanceId, cancellationToken);

        // 2. Protect against side-channel resource enumeration
        // Coalesce the null reference check and ownership tenancy alignment check into a single condition.
        // Uniformly throw an UnauthorizedAccessException to completely obscure whether the ID is missing or unowned.
        if (projectInstance == null || projectInstance.StudentId != _currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("Access Denied: The requested project workspace was not found, or you do not possess ownership authorization permissions.");
        }

        // 3. Invoking the parameterless domain method to update the status code safely
        projectInstance.ConcludeProject();

        // 4. Atomically persist transactional workflow states
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}