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
/// Leverages optimized, untracked relational database projection to compile a read-only snapshot of a student's profile.
/// </summary>
public class GetStudentProfileQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetStudentProfileQuery, StudentProfileDto>
{
    /// <summary>
    /// Processes the student profile query transaction by mapping structural database tables straight to presentation contracts.
    /// </summary>
    /// <param name="request">The structural query container tracking the targeted student identifier.</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be canceled.</param>
    /// <returns>A comprehensive view of the targeted student profile data transfer object layout.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if no student profile record maps to the provided identifier.</exception>
    public async Task<StudentProfileDto> Handle(GetStudentProfileQuery request, CancellationToken cancellationToken)
    {
        // 1. Project the relational database tables directly into clean presentation contracts.
        // Performance Optimization: Direct LINQ .Select projections inherently signal EF Core to perform 
        // explicit SQL INNER/LEFT JOIN statements, eliminating the need for expensive tracking eager loading (.Include).
        var profile = await context.Students
            .AsNoTracking()
            .Where(s => s.Id == request.StudentId) // Aligned with the Guid primary key transformation
            .Select(s => new StudentProfileDto
            {
                Id = s.Id,
                FullName = s.FullName, // Maps the rich aggregate property to the DTO contract
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

        // 2. Throw a precise exception if the requested aggregate boundary is missing
        if (profile == null)
        {
            throw new KeyNotFoundException($"Student profile for ID '{request.StudentId}' was not found within the institutional directory.");
        }

        return profile;
    }
}