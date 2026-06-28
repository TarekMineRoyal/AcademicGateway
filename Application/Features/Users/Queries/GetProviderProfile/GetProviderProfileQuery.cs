using MediatR;

namespace AcademicGateway.Application.Features.Users.Queries.GetProviderProfile;

public record GetProviderProfileQuery(string UserId) : IRequest<ProviderProfileDto>;