using System;
using System.Collections.Generic;

namespace AcademicGateway.Application.Common.Models.AiSearch;

/// <summary>
/// DTO payload sent to POST /api/v1/search/projects for AI vector matchmaking recommendations.
/// </summary>
public class GetProjectRecommendationsQueryModel
{
    /// <summary>
    /// Gets or sets the target academic major name string (required).
    /// </summary>
    public string MajorName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of specialty names associated with the student.
    /// </summary>
    public List<string>? SpecialtyNames { get; set; }

    /// <summary>
    /// Gets or sets the list of skill names possessed by the student.
    /// </summary>
    public List<string>? SkillNames { get; set; }

    /// <summary>
    /// Gets or sets the biographical "about me" context string.
    /// </summary>
    public string? AboutMe { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of recommendation matches to return (default: 10).
    /// </summary>
    public int Limit { get; set; } = 10;

    /// <summary>
    /// Gets or sets the optional major identifier filter.
    /// </summary>
    public Guid? RestrictToMajorId { get; set; }
}