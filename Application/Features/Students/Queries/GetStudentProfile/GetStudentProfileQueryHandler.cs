using AcademicGateway.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.Students.Queries.GetStudentProfile;

/// <summary>
/// Handles the execution of the <see cref="GetStudentProfileQuery"/> request.
/// Leverages optimized, untracked relational database projection to compile a read-only snapshot of a student's profile securely.
/// </summary>
public class GetStudentProfileQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetStudentProfileQuery, StudentProfileDto>
{
    /// <summary>
    /// Processes the student profile query transaction securely by verifying session tenancy parameters.
    /// </summary>
    /// <param name="request">The structural query container tracking the targeted student identifier.</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be canceled.</param>
    /// <returns>A comprehensive view of the targeted student profile data transfer object layout.</returns>
    /// <exception cref="UnauthorizedAccessException">Uniformly thrown if session authentication fails, the resource is missing, or tenancy validation fails.</exception>
    public async Task<StudentProfileDto> Handle(GetStudentProfileQuery request, CancellationToken cancellationToken)
    {
        // Enforce active security session validation early before executing database logic
        if (!currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Access Denied: Authentication is mandatory to query student profiles.");
        }

        // Verify tenancy alignment: Users can only query their own specific student profile context
        if (request.StudentId != currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("Access Denied: The requested profile was not found, or you do not possess read authorization permissions.");
        }

        // Project the relational database tables directly into clean presentation contracts.
        var profile = await context.Students
            .AsNoTracking()
            .Where(s => s.Id == request.StudentId)
            .Select(s => new StudentProfileDto
            {
                Id = s.Id,
                FullName = s.FullName,
                GraduationYear = s.GraduationYear,

                Majors = s.StudentMajors.Select(sm => new StudentMajorDto
                {
                    Id = sm.MajorId,
                    Name = sm.Major != null ? sm.Major.Name : "Unknown Major"
                }).ToList(),

                Specialties = s.StudentSpecialties.Select(ss => new StudentSpecialtyDto
                {
                    Id = ss.SpecialtyId,
                    Name = ss.Specialty != null ? ss.Specialty.Name : "Unknown Specialty"
                }).ToList(),

                Skills = s.StudentSkills.Select(sk => new StudentSkillDto
                {
                    Id = sk.SkillId,
                    Name = sk.Skill != null ? sk.Skill.Name : "Unknown Skill"
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        // Validate presence boundaries uniformly to protect against resource scanning behaviors
        if (profile == null)
        {
            throw new UnauthorizedAccessException("Access Denied: The requested profile was not found, or you do not possess read authorization permissions.");
        }

        return profile;
    }
}