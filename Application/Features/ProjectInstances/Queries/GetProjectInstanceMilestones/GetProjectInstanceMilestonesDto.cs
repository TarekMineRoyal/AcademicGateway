using AcademicGateway.Domain.Common.Enums;
using AcademicGateway.Domain.ProjectInstances.Enums;
using System;
using System.Collections.Generic;

namespace AcademicGateway.Application.Features.ProjectInstances.Queries.GetProjectInstanceMilestones;

/// <summary>
/// Data transfer object representing the complete runtime snapshot execution matrix, including scheduling deadlines,
/// weights, balance states, and relationship edges for active local milestone tracking nodes.
/// </summary>
public record ProjectInstanceMilestoneDto
{
    /// <summary>
    /// Gets the unique tracking identifier for this specific localized runtime milestone instance.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the unique identifier of the parent ProjectInstance aggregate root workspace execution context.
    /// </summary>
    public Guid ProjectInstanceId { get; init; }

    /// <summary>
    /// Gets the descriptive headline text snapshot extracted from the blueprint design layout.
    /// </summary>
    public string TitleSnapshot { get; init; } = string.Empty;

    /// <summary>
    /// Gets the descriptive instructions and criteria requirements copy pulled down from the template layout.
    /// </summary>
    public string DescriptionSnapshot { get; init; } = string.Empty;

    /// <summary>
    /// Gets the nominal effort estimation requirement index value calculated in hours.
    /// </summary>
    public decimal ExpectedEffortInHours { get; init; }

    /// <summary>
    /// Gets the current execution state within the individual step state machine configuration.
    /// </summary>
    public LocalMilestoneStatus Status { get; init; }

    /// <summary>
    /// Gets the student-assigned work kickoff date for this phase, or null if unallocated.
    /// </summary>
    public DateTime? ScheduledStartDate { get; init; }

    /// <summary>
    /// Gets the student-assigned deadline target timestamp constraint, or null if unallocated.
    /// </summary>
    public DateTime? ScheduledEndDate { get; init; }

    /// <summary>
    /// Gets the operational work breakdown structure (WBS) weight percentage relative to total project effort.
    /// </summary>
    public decimal WbsWeight { get; init; }

    /// <summary>
    /// Gets the academic grading score weight contribution percentage relative to total score.
    /// </summary>
    public decimal GradingWeight { get; init; }

    /// <summary>
    /// Gets a value indicating whether the internal nested task breakdown weights sum up accurately to 100%.
    /// </summary>
    public bool IsWbsBalanced { get; init; }

    /// <summary>
    /// Gets the collection of active runtime execution dependency constraints targeting this specific node checkpoint.
    /// </summary>
    public List<LocalMilestoneDependencyDto> InboundDependencies { get; init; } = [];

    /// <summary>
    /// Gets the hierarchical child collection of nested tasks carrying individual milestones breakdown and performance metrics.
    /// </summary>
    public List<LocalTaskDto> Tasks { get; init; } = [];
}

/// <summary>
/// Sub-DTO tracking runtime execution variables and grading states for a localized task nested under an active milestone tracking node.
/// </summary>
public record LocalTaskDto(
    Guid Id,
    string TitleSnapshot,
    string DescriptionSnapshot,
    decimal Weight,
    DeliverableType RequiredDeliverableType,
    LocalTaskStatus Status,
    string? SubmissionPayload,
    DateTime? SubmittedAt,
    decimal? Grade,
    string? EvaluationFeedback,
    DateTime? GradedAt);

/// <summary>
/// Sub-DTO tracking runtime sequencing relationship constraints between localized project milestones.
/// </summary>
public record LocalMilestoneDependencyDto(
    Guid PredecessorId,
    Guid SuccessorId,
    DependencyType Type);