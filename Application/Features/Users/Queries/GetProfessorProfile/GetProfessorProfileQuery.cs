using MediatR;

namespace AcademicGateway.Application.Features.Users.Queries.GetProfessorProfile;

public record GetProfessorProfileQuery(string UserId) : IRequest<ProfessorProfileDto>;