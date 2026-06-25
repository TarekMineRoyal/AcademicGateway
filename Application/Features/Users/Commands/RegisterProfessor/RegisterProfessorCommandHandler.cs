using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.Entities;
using MediatR;

namespace AcademicGateway.Application.Features.Users.Commands.RegisterProfessor;

public class RegisterProfessorCommandHandler(
    IIdentityService identityService,
    IApplicationDbContext dbContext)
    : IRequestHandler<RegisterProfessorCommand, string>
{
    public async Task<string> Handle(RegisterProfessorCommand request, CancellationToken cancellationToken)
    {
        // 1. Create the base Identity User
        var (succeeded, userId, errors) = await identityService.CreateUserAsync(
            request.Username,
            request.Email,
            request.Password);

        if (!succeeded)
        {
            throw new Exception($"Professor creation failed: {string.Join(", ", errors)}");
        }

        // 2. Create the Domain Profile
        var professorProfile = new Professor
        {
            UserId = userId,
            AcademicDepartment = request.AcademicDepartment
        };

        dbContext.Professors.Add(professorProfile);

        // 3. Save to database
        await dbContext.SaveChangesAsync(cancellationToken);

        return userId;
    }
}