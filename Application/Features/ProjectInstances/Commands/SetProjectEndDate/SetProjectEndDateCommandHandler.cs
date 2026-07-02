using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AcademicGateway.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGateway.Application.Features.ProjectInstances.Commands.SetProjectEndDate;

/// <summary>
/// Orchestrates the application logic for adjusting a project instance's scheduled completion date
/// by delegating security validation and state modifications to the domain aggregate root.
/// </summary>
public class SetProjectEndDateCommandHandler : IRequestHandler<SetProjectEndDateCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetProjectEndDateCommandHandler"/> class.
    /// </summary>
    public SetProjectEndDateCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Fetches the live workspace aggregate, verifies identity presence, and triggers the domain rule method.
    /// </summary>
    public async Task<Unit> Handle(SetProjectEndDateCommand request, CancellationToken cancellationToken)
    {
        // 1. Retrieve the target project instance workspace aggregate root
        var projectInstance = await _context.ProjectInstances
            .FirstOrDefaultAsync(pi => pi.Id == request.ProjectInstanceId, cancellationToken);

        if (projectInstance == null)
        {
            throw new KeyNotFoundException($"The target project instance workspace with ID '{request.ProjectInstanceId}' was not found.");
        }

        // 2. Pre-routing Authentication Guard
        if (!_currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Access Denied: You must be authenticated to alter workspace deadlines.");
        }

        // 3. Invoke the explicit domain behavior method established in Step 1.7.
        // This naturally passes the executor's ID down so the aggregate root can guard its own rules.
        projectInstance.SetProjectEndDate(request.NewEndDate, _currentUserService.UserId ?? Guid.Empty);

        // 4. Flush the state mutations down to physical rows atomically
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}