using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.Entities;
using MediatR;

namespace AcademicGateway.Application.Features.Users.Commands.RegisterStudent;

// Note: Using C# 12 Primary Constructors here for dependency injection
public class RegisterStudentCommandHandler(
    IIdentityService identityService,
    IApplicationDbContext dbContext)
    : IRequestHandler<RegisterStudentCommand, string>
{
    public async Task<string> Handle(RegisterStudentCommand request, CancellationToken cancellationToken)
    {
        // 1. Create the base Identity User
        var (succeeded, userId, errors) = await identityService.CreateUserAsync(
            request.Username,
            request.Email,
            request.Password);

        if (!succeeded)
        {
            // In a real app, you might throw a custom ValidationException here
            throw new Exception($"User creation failed: {string.Join(", ", errors)}");
        }

        // 2. Create the Domain Profile using the generated UserId
        var studentProfile = new Student
        {
            UserId = userId,
            Major = request.Major,
            Specialty = request.Specialty,
            GraduationYear = request.GraduationYear
        };

        dbContext.Students.Add(studentProfile);

        // 3. Save to database
        await dbContext.SaveChangesAsync(cancellationToken);

        // Return the new User ID to the API controller
        return userId;
    }
}