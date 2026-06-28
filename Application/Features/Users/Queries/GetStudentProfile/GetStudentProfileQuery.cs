using MediatR;

namespace AcademicGateway.Application.Features.Users.Queries.GetStudentProfile;

public record GetStudentProfileQuery(string UserId) : IRequest<StudentProfileDto>;