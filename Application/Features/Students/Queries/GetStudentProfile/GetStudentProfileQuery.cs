using AcademicGateway.Application.Features.Students.Queries.GetStudentProfile;
using MediatR;
using System;

namespace AcademicGateway.Application.Features.Students.Queries.GetStudentProfile;

/// <summary>
/// CQRS Query to retrieve the comprehensive academic profile, mapped major programs, 
/// technical specialties, and claimed skill inventories for a specific student.
/// </summary>
/// <param name="StudentId">The unique global identifier of the student profile, mapping 1:1 to their security identity context.</param>
public record GetStudentProfileQuery(Guid StudentId) : IRequest<StudentProfileDto>;