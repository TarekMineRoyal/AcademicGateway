using AcademicGateway.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProviderApplications.Commands.ResubmitProviderApplication;

/// <summary>
/// Orchestrates the business command pipeline for modifying and resetting a rejected <see cref="ProviderApplication"/> back into the verification pool.
/// Guarded completely against Broken Object Level Authorization (BOLA) and side-channel security threats.
/// </summary>
public class ResubmitProviderApplicationCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<ResubmitProviderApplicationCommand, Guid>
{
    /// <summary>
    /// Processes the company registration resubmission payload, updating aggregate records and advancing state machine gates cleanly.
    /// </summary>
    /// <param name="request">The incoming command carrying updated corporate portfolios and verification link targets.</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be canceled.</param>
    /// <returns>The primary tracking identity key code of the resubmitted application registry form.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown if validation tokens fail authentication or cross-resource user matches fail context parameters.</exception>
    public async Task<Guid> Handle(ResubmitProviderApplicationCommand request, CancellationToken cancellationToken)
    {
        // 1. Enforce active authorization validation checks early before executing data tier operations
        if (!currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Access Denied: You must be fully authenticated to alter corporate onboarding records.");
        }

        // 2. Validate cross-cutting authorization bounds to prevent multi-tenant data bleed (BOLA)
        // Ensure that the target actor context parameter aligns strictly with the token payload metadata context.
        if (request.ProviderId != currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("Access Denied: You do not possess structural permissions to manage modifications for this provider account record.");
        }

        // 3. Retrieve the matching corporate provider verification application form from persistent storage
        var application = await context.ProviderApplications
            .FirstOrDefaultAsync(a => a.ProviderId == request.ProviderId, cancellationToken);

        // 4. Validate system profile row presence and mask metadata directory tracking configurations
        // If the application instance does not exist, throw a generic authorization error rather than a descriptive 404 block.
        // This stops malicious scanning vectors from mapping out institutional partner records through serial guessing.
        if (application == null)
        {
            throw new UnauthorizedAccessException("Access Denied: The requested verification record was not found or remains unavailable to the active user profile.");
        }

        // 5. Delegate structural data updates and validation resets to the encapsulated aggregate root method
        // This native block re-evaluates the internal state, purges prior tracking reviewer references,
        // and safely transitions the status indicator state flag back into Pending review status.
        application.Resubmit(request.CompanyDetails, request.VerificationDocumentsUrl);

        // 6. Persist atomic adjustments and dispatch domain notification structures down the event framework pipeline
        await context.SaveChangesAsync(cancellationToken);

        // 7. Hand back the verification primary identifier key
        return application.Id;
    }
}