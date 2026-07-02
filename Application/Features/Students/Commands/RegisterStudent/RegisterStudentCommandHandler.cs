using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.Students;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.Students.Commands.RegisterStudent;

/// <summary>
/// Handles the transaction routine to process a <see cref="RegisterStudentCommand"/>.
/// Provisions centralized user identity credentials and initializes a rich Student aggregate root record.
/// </summary>
public class RegisterStudentCommandHandler(
    IIdentityService identityService,
    IApplicationDbContext dbContext)
    : IRequestHandler<RegisterStudentCommand, Guid>
{
    /// <summary>
    /// Processes identity profile generation, validates invariants, applies academic mappings, and flushes states.
    /// </summary>
    /// <param name="request">The structural parameter bundle tracking requested credentials and registration specifications.</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be canceled.</param>
    /// <returns>The newly assigned tracking Guid identifying the materialized Student aggregate root entity.</returns>
    /// <exception cref="InvalidOperationException">Thrown if identity profile creation fails via infrastructure layer restrictions.</exception>
    public async Task<Guid> Handle(RegisterStudentCommand request, CancellationToken cancellationToken)
    {
        // 1. Create the secure baseline application identity context user
        var (succeeded, userId, errors) = await identityService.CreateUserAsync(
            request.Username,
            request.Email,
            request.Password);

        if (!succeeded)
        {
            throw new InvalidOperationException($"Student identity configuration failed: {string.Join(", ", errors)}");
        }

        // 2. Enforce Domain Encapsulation - Initialize via explicit parameterized constructor logic.
        // This guarantees all aggregate validation invariants fire cleanly prior to persistence layer mapping.
        var studentProfile = new Student(
            id: userId,
            fullName: request.FullName,
            graduationYear: request.GraduationYear
        );

        // 3. Attach academic programs via DDD behavioral methods
        // Architectural Optimization: Because the RegisterStudentCommandValidator pre-checks inputs, 
        // we map these collections directly to shield code from raw join-table operations.
        if (request.MajorIds != null)
        {
            foreach (var majorId in request.MajorIds)
            {
                studentProfile.AddMajor(majorId);
            }
        }

        // 4. Attach fine-grained structural educational sub-specialties
        if (request.SpecialtyIds != null)
        {
            foreach (var specialtyId in request.SpecialtyIds)
            {
                studentProfile.AddSpecialty(specialtyId);
            }
        }

        // 5. Populate student technical capability or competency skill inventories
        if (request.SkillIds != null)
        {
            foreach (var skillId in request.SkillIds)
            {
                studentProfile.AddSkill(skillId);
            }
        }

        // 6. Queue the tracked aggregate root structure for relational database persistence
        dbContext.Students.Add(studentProfile);

        // 7. Commit unit of work transactions securely across all intersection tracking rows
        await dbContext.SaveChangesAsync(cancellationToken);

        return userId;
    }
}