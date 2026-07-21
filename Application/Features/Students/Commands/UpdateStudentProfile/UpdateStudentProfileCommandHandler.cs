using AcademicGateway.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.Students.Commands.UpdateStudentProfile;

/// <summary>
/// Handles the transaction routine to process an <see cref="UpdateStudentProfileCommand"/>.
/// Resolves the student aggregate via the secure session execution context, updates core data properties, 
/// synchronizes child relational matrices, and flushes states cleanly.
/// </summary>
public class UpdateStudentProfileCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<UpdateStudentProfileCommand>
{
    /// <summary>
    /// Processes the student profile update, executes domain mutations, synchronizes aggregate collections, and saves changes.
    /// </summary>
    /// <param name="request">The structural data bundle carrying the updated profile properties and selection lookups.</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be canceled.</param>
    /// <returns>A completed asynchronous execution task.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown if the security session is unauthenticated or the underlying profile does not exist.</exception>
    public async Task Handle(UpdateStudentProfileCommand request, CancellationToken cancellationToken)
    {
        // 1. Enforce active security session validation early before querying data pools
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("Access Denied: Authentication is mandatory to update profile metrics.");
        }

        Guid studentId = currentUserService.UserId.Value;

        // 2. Retrieve the encapsulated Student aggregate root including all tracked child collections
        var student = await context.Students
            .Include(s => s.StudentMajors)
            .Include(s => s.StudentSkills)
            .Include(s => s.StudentSpecialties)
            .FirstOrDefaultAsync(s => s.Id == studentId, cancellationToken);

        // 3. Prevent side-channel exposure or mapping vulnerabilities by using a uniform exception boundary
        if (student == null)
        {
            throw new UnauthorizedAccessException("Access Denied: Authorized profile context was not found.");
        }

        // 4. Mutate core primitive attributes via formal domain methods
        student.UpdateFullName(request.FullName);
        student.UpdateGraduationYear(request.GraduationYear);
        student.UpdateAboutMe(request.AboutMe);

        // 5. Synchronize Academic Major Alignments (DDD Differential Synchronization Pattern)
        var targetMajors = request.MajorIds ?? Array.Empty<Guid>();
        var currentMajors = student.StudentMajors.Select(sm => sm.MajorId).ToList();

        foreach (var majorId in targetMajors.Except(currentMajors))
        {
            student.AddMajor(majorId);
        }

        foreach (var majorId in currentMajors.Except(targetMajors))
        {
            student.RemoveMajor(majorId);
        }

        // 6. Synchronize Educational Sub-Specialties
        var targetSpecialties = request.SpecialtyIds ?? Array.Empty<Guid>();
        var currentSpecialties = student.StudentSpecialties.Select(ss => ss.SpecialtyId).ToList();

        foreach (var specialtyId in targetSpecialties.Except(currentSpecialties))
        {
            student.AddSpecialty(specialtyId);
        }

        foreach (var specialtyId in currentSpecialties.Except(targetSpecialties))
        {
            student.RemoveSpecialty(specialtyId);
        }

        // 7. Synchronize Technical Capability/Skill Inventories
        var targetSkills = request.SkillIds ?? Array.Empty<Guid>();
        var currentSkills = student.StudentSkills.Select(ss => ss.SkillId).ToList();

        foreach (var skillId in targetSkills.Except(currentSkills))
        {
            student.AddSkill(skillId);
        }

        foreach (var skillId in currentSkills.Except(targetSkills))
        {
            student.RemoveSkill(skillId);
        }

        // 8. Commit outstanding aggregate alterations atomically down to relational storage structures
        await context.SaveChangesAsync(cancellationToken);
    }
}