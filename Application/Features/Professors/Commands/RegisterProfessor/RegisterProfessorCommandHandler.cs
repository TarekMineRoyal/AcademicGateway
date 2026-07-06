using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.Professors;
using AcademicGateway.Domain.Professors.Exceptions;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.Professors.Commands.RegisterProfessor;

/// <summary>
/// Handles the transaction routine to process a <see cref="RegisterProfessorCommand"/>.
/// Orchestrates cross-cutting identity generation and securely invokes the aggregate initialization lifecycle sequence.
/// </summary>
public class RegisterProfessorCommandHandler(
    IIdentityService identityService,
    IApplicationDbContext dbContext)
    : IRequestHandler<RegisterProfessorCommand, Guid>
{
    /// <summary>
    /// Processes user record provisioning, parses identities, hydrates domain properties, and flushes states to storage.
    /// </summary>
    /// <param name="request">The structural parameter bundle tracking requested credentials and registration specifications.</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be canceled.</param>
    /// <returns>The newly assigned tracking Guid identifying the materialized Professor aggregate root entity.</returns>
    /// <exception cref="InvalidOperationException">Thrown if security profile generation fails via identity credential restrictions.</exception>
    /// <exception cref="InvalidProfessorDetailsException">Thrown if core textual inputs mismatch baseline system rules.</exception>
    /// <exception cref="InvalidSupervisionCapacityException">Thrown if assigned project bounds evaluate below threshold scales.</exception>
    public async Task<Guid> Handle(RegisterProfessorCommand request, CancellationToken cancellationToken)
    {
        // 1. Create the secure baseline application identity context user
        var (succeeded, userId, errors) = await identityService.CreateUserAsync(
            request.Username,
            request.Email,
            request.Password,
            "Professor");

        if (!succeeded)
        {
            throw new InvalidOperationException($"Professor credential provisioning failed: {string.Join(", ", errors)}");
        }

        // 2. Enforce Domain Encapsulation - Initialize via explicit parameterized constructor logic.
        // userId is already a strongly typed Guid coming back from IIdentityService.
        var professorProfile = new Professor(
            id: userId,
            fullName: request.FullName,
            department: request.AcademicDepartment,
            rank: request.Rank,
            maxSupervisionCapacity: request.MaxSupervisionCapacity
        );

        // 3. Attach entity boundary snapshot directly to tracking sets
        dbContext.Professors.Add(professorProfile);

        // 4. Commit unit of work transactions securely
        await dbContext.SaveChangesAsync(cancellationToken);

        return userId;
    }
}