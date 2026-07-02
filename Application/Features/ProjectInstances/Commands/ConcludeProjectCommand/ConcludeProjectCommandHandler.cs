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
        var projectInstance = await _context.ProjectInstances
            .FirstOrDefaultAsync(pi => pi.Id == request.ProjectInstanceId, cancellationToken);

        if (projectInstance == null)
        {
            throw new KeyNotFoundException($"The target project instance workspace with ID '{request.ProjectInstanceId}' was not found.");
        }

        // Security Boundary: Only the specific student owner who initialized the run can wrap it up
        if (!_currentUserService.IsAuthenticated || projectInstance.StudentId != _currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("Access Denied: Only the student owner can mark this project workspace as concluded.");
        }

        // Invoking the parameterless domain method to update the status code
        projectInstance.ConcludeProject();

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}