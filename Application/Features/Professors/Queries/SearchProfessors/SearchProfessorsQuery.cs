using MediatR;
using System.Collections.Generic;

namespace AcademicGateway.Application.Features.Professors.Queries.SearchProfessors;

/// <summary>
/// MediatR query parameter envelope passing criteria down to the faculty directory search engine.
/// Wraps optional search parameters used to discover matching professor profiles.
/// </summary>
/// <param name="SearchTerm">The optional structural keyword segment matched case-insensitively against user details.</param>
public record SearchProfessorsQuery(string? SearchTerm) : IRequest<IReadOnlyCollection<ProfessorSearchResultDto>>;