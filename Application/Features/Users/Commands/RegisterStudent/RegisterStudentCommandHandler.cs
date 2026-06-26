using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.Entities;
using MediatR;

namespace AcademicGateway.Application.Features.Users.Commands.RegisterStudent;

public class RegisterStudentCommandHandler(
    IIdentityService identityService,
    IApplicationDbContext dbContext)
    : IRequestHandler<RegisterStudentCommand, string>
{
    public async Task<string> Handle(RegisterStudentCommand request, CancellationToken cancellationToken)
    {
        var (succeeded, userId, errors) = await identityService.CreateUserAsync(
            request.Username,
            request.Email,
            request.Password);

        if (!succeeded) throw new Exception($"User creation failed: {string.Join(", ", errors)}");

        // 1. Create the base profile
        var studentProfile = new Student
        {
            UserId = userId,
            GraduationYear = request.GraduationYear
        };

        // 2. Map the Majors
        if (request.MajorIds.Any())
        {
            foreach (var majorId in request.MajorIds)
            {
                studentProfile.StudentMajors.Add(new StudentMajor
                {
                    StudentId = userId,
                    MajorId = majorId
                });
            }
        }

        // 3. Map the Specialties
        if (request.SpecialtyIds.Any())
        {
            foreach (var specialtyId in request.SpecialtyIds)
            {
                studentProfile.StudentSpecialties.Add(new StudentSpecialty
                {
                    StudentId = userId,
                    SpecialtyId = specialtyId
                });
            }
        }

        // 4. Map the Skills
        if (request.SkillIds.Any())
        {
            foreach (var skillId in request.SkillIds)
            {
                studentProfile.StudentSkills.Add(new StudentSkill
                {
                    StudentId = userId,
                    SkillId = skillId
                });
            }
        }

        dbContext.Students.Add(studentProfile);
        await dbContext.SaveChangesAsync(cancellationToken);

        return userId;
    }
}