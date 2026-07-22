using System;
using System.Collections.Generic;

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

    /// <summary>
    /// Gets or sets the target academic department division designation text.
    /// </summary>
    public string Department { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional biographical summary text for the professor profile.
    /// </summary>
    public string? AboutMe { get; set; }

    /// <summary>
    /// Gets or sets the collection of research interest topic areas mapped to this professor.
    /// </summary>
    public IReadOnlyCollection<string> ResearchInterests { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the current number of active projects supervised by this professor.
    /// </summary>
    public int CurrentProjectCount { get; set; }

    /// <summary>
    /// Gets or sets the maximum project supervision capacity limit for this professor.
    /// </summary>
    public int MaxSupervisionCapacity { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this professor is currently accepting new student projects.
    /// </summary>
    public bool IsAcceptingProjects { get; set; }
}