using AcademicGateway.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.Users.Queries.GetStudentProfile;

public class GetStudentProfileQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetStudentProfileQuery, StudentProfileDto>
{
    public async Task<StudentProfileDto> Handle(GetStudentProfileQuery request, CancellationToken cancellationToken)
    {
        var profile = await context.Students
            .AsNoTracking()
            .Where(s => s.UserId == request.UserId)
            .Select(s => new StudentProfileDto
            {
                UserId = s.UserId,
                GraduationYear = s.GraduationYear,
                Majors = s.StudentMajors.Select(sm => new StudentMajorDto
                {
                    Id = sm.MajorId,
                    Name = sm.Major.Name
                }).ToList(),
                Specialties = s.StudentSpecialties.Select(ss => new StudentSpecialtyDto
                {
                    Id = ss.SpecialtyId,
                    Name = ss.Specialty.Name
                }).ToList(),
                Skills = s.StudentSkills.Select(ss => new StudentSkillDto
                {
                    Id = ss.SkillId,
                    Name = ss.Skill.Name
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (profile == null)
        {
            throw new KeyNotFoundException($"Student profile for User ID '{request.UserId}' was not found.");
        }

        return profile;
    }
}