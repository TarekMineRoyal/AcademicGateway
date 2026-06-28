using AcademicGateway.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.Users.Queries.GetProfessorProfile;

public class GetProfessorProfileQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetProfessorProfileQuery, ProfessorProfileDto>
{
    public async Task<ProfessorProfileDto> Handle(GetProfessorProfileQuery request, CancellationToken cancellationToken)
    {
        var profile = await context.Professors
            .AsNoTracking()
            .Where(p => p.UserId == request.UserId)
            .Select(p => new ProfessorProfileDto
            {
                UserId = p.UserId,
                AcademicDepartment = p.AcademicDepartment
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (profile == null)
        {
            throw new KeyNotFoundException($"Professor profile for User ID '{request.UserId}' was not found.");
        }

        return profile;
    }
}