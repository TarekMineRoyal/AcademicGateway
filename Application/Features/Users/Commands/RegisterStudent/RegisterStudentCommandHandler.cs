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

        // Create the profile
        var studentProfile = new Student
        {
            UserId = userId,
            Major = request.Major,
            Specialty = request.Specialty,
            GraduationYear = request.GraduationYear
        };

        // Map the skills
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