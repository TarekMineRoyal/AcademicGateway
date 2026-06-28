using AcademicGateway.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.Users.Queries.GetProviderProfile;

public class GetProviderProfileQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetProviderProfileQuery, ProviderProfileDto>
{
    public async Task<ProviderProfileDto> Handle(GetProviderProfileQuery request, CancellationToken cancellationToken)
    {
        var profile = await context.Providers
            .AsNoTracking()
            .Where(p => p.UserId == request.UserId)
            .Select(p => new ProviderProfileDto
            {
                UserId = p.UserId,
                OrganizationName = p.OrganizationName,
                Industry = p.Industry,
                WebsiteUrl = p.WebsiteUrl,
                IsVerified = p.IsVerified
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (profile == null)
        {
            throw new KeyNotFoundException($"Provider profile for User ID '{request.UserId}' was not found.");
        }

        return profile;
    }
}