using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AcademicGateway.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGateway.Application.Features.ProjectInstances.Commands.CancelProject;

/// <summary>
/// Handles the orchestration flow to prematurely abort or cancel an active running project workspace.
/// Fortified against Broken Object Level Authorization (BOLA) and side-channel resource enumeration vectors.
/// </summary>
public class CancelProjectCommandHandler : IRequestHandler<CancelProjectCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CancelProjectCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(CancelProjectCommand request, CancellationToken cancellationToken)
    {
        // 1. Enforce active session presence before hitting data persistence layers
        if (!_currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Access Denied: Authentication is mandatory to adjust workspace lifecycle states.");
        }

        var projectInstance = await _context.ProjectInstances
            .FirstOrDefaultAsync(pi => pi.Id == request.ProjectInstanceId, cancellationToken);

        // 2. Protect against side-channel resource enumeration
        // Uniformly throw an UnauthorizedAccessException if the record is missing OR owned by another entity.
        // This hides the system's internal object existence parameters from scanning behaviors.
        if (projectInstance == null || projectInstance.StudentId != _currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("Access Denied: The requested project workspace was not found, or you do not possess ownership authorization permissions.");
        }

        // 3. Invoke the encapsulated domain lifecycle transition rules
        projectInstance.CancelProject();

        // 4. Atomically persist state tracking shifts
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}