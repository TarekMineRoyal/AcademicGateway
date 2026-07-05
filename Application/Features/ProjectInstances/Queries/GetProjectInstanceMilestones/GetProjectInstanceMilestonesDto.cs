using AcademicGateway.Domain.Common.Enums;
using AcademicGateway.Domain.ProjectInstances.Enums;
using System;
using System.Collections.Generic;

namespace AcademicGateway.Application.Features.ProjectInstances.Queries.GetProjectInstanceMilestones;

/// <summary>
/// Data transfer object representing the complete runtime snapshot execution matrix, including scheduling deadlines,
/// delivery payloads, evaluation outcomes, and relationship edges for active local milestone tracking nodes.
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
    /// Gets the verification format category constraint token mapping student payload submissions.
    /// </summary>
    public DeliverableType RequiredDeliverableType { get; init; }

    /// <summary>
    /// Gets the current execution state within the individual step state machine configuration.
    /// </summary>
    public LocalMilestoneStatus Status { get; init; }

    /// <summary>
    /// Gets the student-assigned work kickoff date for this phase, or null if unallocated per Rule 3.
    /// </summary>
    public DateTime? ScheduledStartDate { get; init; }

    /// <summary>
    /// Gets the student-assigned deadline target timestamp constraint, or null if unallocated per Rule 3.
    /// </summary>
    public DateTime? ScheduledEndDate { get; init; }

    /// <summary>
    /// Gets the polymorphic raw text copy or storage asset pointer representing the student's deliverable push.
    /// </summary>
    public string? SubmissionPayload { get; init; }

    /// <summary>
    /// Gets the precise synchronized tracking timestamp indicating when the upload submission was committed.
    /// </summary>
    public DateTime? SubmittedAt { get; init; }

    /// <summary>
    /// Gets the numerical evaluation performance score value awarded by the grading faculty mentor.
    /// </summary>
    public decimal? Grade { get; init; }

    /// <summary>
    /// Gets the qualitative formal feedback commentary text logged by the grading faculty member.
    /// </summary>
    public string? EvaluationFeedback { get; init; }

    /// <summary>
    /// Gets the point-in-time timestamp coordinate tracking when evaluation modifications were frozen.
    /// </summary>
    public DateTime? GradedAt { get; init; }

    /// <summary>
    /// Gets the collection of active runtime execution dependency constraints targeting this specific node checkpoint.
    /// </summary>
    public List<LocalMilestoneDependencyDto> InboundDependencies { get; init; } = [];
}

/// <summary>
/// Sub-DTO tracking runtime sequencing relationship constraints between localized project milestones.
/// </summary>
public record LocalMilestoneDependencyDto(
    Guid PredecessorId,
    Guid SuccessorId,
    DependencyType Type);