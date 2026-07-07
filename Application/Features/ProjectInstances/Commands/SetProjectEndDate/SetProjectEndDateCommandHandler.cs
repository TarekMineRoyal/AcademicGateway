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
/// by delegating security validation and state modifications to the domain aggregate root securely.
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
    /// Fetches the live workspace aggregate, verifies identity presence, and triggers the domain rule method securely.
    /// </summary>
    /// <param name="request">The incoming command container tracking the target workspace and new date parameters.</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be canceled.</param>
    /// <returns>A MediatR completion compliance unit instance.</returns>
    /// <exception cref="UnauthorizedAccessException">Uniformly thrown if session authentication fails, the resource is missing, or tenancy validation fails.</exception>
    public async Task<Unit> Handle(SetProjectEndDateCommand request, CancellationToken cancellationToken)
    {
        // Enforce active session validation early before executing database logic
        if (!_currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Access Denied: You must be authenticated to alter workspace deadlines.");
        }

        // Retrieve the target project instance workspace aggregate root
        var projectInstance = await _context.ProjectInstances
            .FirstOrDefaultAsync(pi => pi.Id == request.ProjectInstanceId, cancellationToken);

        // Validate presence boundaries and verify resource tenancy uniformly.
        // Restricting initial execution to the bound student, supervisor, or provider masks resource presence indicators.
        if (projectInstance == null || (projectInstance.StudentId != _currentUserService.UserId &&
                                        projectInstance.SupervisorId != _currentUserService.UserId &&
                                        projectInstance.ProviderId != _currentUserService.UserId))
        {
            throw new UnauthorizedAccessException("Access Denied: The requested project workspace was not found, or you do not possess deadline management authorization permissions.");
        }

        // Invoke the explicit domain behavior method
        // This passes the verified executor's ID down so the aggregate root can guard fine-grained business rules.
        projectInstance.SetProjectEndDate(request.NewEndDate, _currentUserService.UserId ?? Guid.Empty);

        // Flush the state mutations down to physical rows atomically
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}