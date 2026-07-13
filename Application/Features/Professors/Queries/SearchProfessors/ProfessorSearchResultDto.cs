using System;

namespace AcademicGateway.Application.Features.Professors.Queries.SearchProfessors;

/// <summary>
/// Represents a lightweight, presentational data transfer object for professor search lookup results.
/// Maps directly to the flat object structure required by the frontend initialization pipeline autocomplete flow.
/// </summary>
public class ProfessorSearchResultDto
{
    /// <summary>
    /// Gets or sets the unique user identity tracking key matching this professor profile context.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the legal full name string matching this institutional faculty member.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique corporate or institutional contact electronic mail address mapped to the account.
    /// </summary>
    public string Email { get; set; } = string.Empty;
}