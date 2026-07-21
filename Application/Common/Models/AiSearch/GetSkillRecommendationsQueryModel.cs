using System;
using System.Collections.Generic;

namespace AcademicGateway.Application.Common.Models.AiSearch;

/// <summary>
/// DTO payload sent to POST /api/v1/search/skills for adjacent skill recommendations.
/// </summary>
public class GetSkillRecommendationsQueryModel
{
    /// <summary>
    /// Gets or sets the primary major name text (required).
    /// </summary>
    public string MajorName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of current student specialty names.
    /// </summary>
    public List<string>? SpecialtyNames { get; set; }

    /// <summary>
    /// Gets or sets the list of current student skill names.
    /// </summary>
    public List<string>? SkillNames { get; set; }

    /// <summary>
    /// Gets or sets the biographical "about me" context string.
    /// </summary>
    public string? AboutMe { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of skill recommendations to return (default: 10).
    /// </summary>
    public int Limit { get; set; } = 10;
}