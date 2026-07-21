using System;
using System.Collections.Generic;

namespace AcademicGateway.Application.Common.Models.AiSearch;

/// <summary>
/// DTO payload sent to POST /api/v1/search/professors for AI vector faculty advisor suggestions.
/// </summary>
public class GetProfessorSuggestionsQueryModel
{
    /// <summary>
    /// Gets or sets the project title context (required).
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project description context (required).
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target major name text.
    /// </summary>
    public string? MajorName { get; set; }

    /// <summary>
    /// Gets or sets the target specialty name text.
    /// </summary>
    public string? SpecialtyName { get; set; }

    /// <summary>
    /// Gets or sets the list of required skill names.
    /// </summary>
    public List<string>? SkillNames { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of suggestion matches to return (default: 10).
    /// </summary>
    public int Limit { get; set; } = 10;
}