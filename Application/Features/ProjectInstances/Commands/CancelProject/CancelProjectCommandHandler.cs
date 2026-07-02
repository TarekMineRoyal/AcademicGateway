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
        var projectInstance = await _context.ProjectInstances
            .FirstOrDefaultAsync(pi => pi.Id == request.ProjectInstanceId, cancellationToken);

        if (projectInstance == null)
        {
            throw new KeyNotFoundException($"The target project instance workspace with ID '{request.ProjectInstanceId}' was not found.");
        }

        // Security Boundary: Only the student owner who initialized the run has the authority to abandon it
        if (!_currentUserService.IsAuthenticated || projectInstance.StudentId != _currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("Access Denied: Only the student owner can cancel this project workspace.");
        }

        // Invoking the parameterless domain method to trigger the lifecycle state-machine
        projectInstance.CancelProject();

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}