using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AcademicGateway.Application.Common.Models.AiSearch;
using AcademicGateway.Application.Common.Models.AiSync;

namespace AcademicGateway.Application.Common.Interfaces;

/// <summary>
/// Abstraction for communicating with the external AI Matchmaking Engine microservice 
/// for data index synchronization, vector search, and recommendation queries.
/// </summary>
public interface IAiMatchmakingClient
{
    /// <summary>
    /// Synchronizes a student profile vector representation in the AI engine index.
    /// </summary>
    /// <param name="student">The student synchronization model.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task SyncStudentAsync(StudentSyncModel student, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a student profile vector representation from the AI engine index.
    /// </summary>
    /// <param name="studentId">The unique identifier of the student to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task DeleteStudentAsync(Guid studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronizes a professor profile vector representation in the AI engine index.
    /// </summary>
    /// <param name="professor">The professor synchronization model.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task SyncProfessorAsync(ProfessorSyncModel professor, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a professor profile vector representation from the AI engine index.
    /// </summary>
    /// <param name="professorId">The unique identifier of the professor to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task DeleteProfessorAsync(Guid professorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronizes a project template vector representation in the AI engine index.
    /// </summary>
    /// <param name="project">The project template synchronization model.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task SyncProjectAsync(ProjectSyncModel project, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a project template vector representation from the AI engine index.
    /// </summary>
    /// <param name="projectId">The unique identifier of the project template to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task DeleteProjectAsync(Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronizes a skill vector representation in the AI engine index.
    /// </summary>
    /// <param name="skill">The skill synchronization model.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task SyncSkillAsync(SkillSyncModel skill, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a skill vector representation from the AI engine index.
    /// </summary>
    /// <param name="skillId">The unique identifier of the skill to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task DeleteSkillAsync(Guid skillId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a bulk backfill/synchronization of student profile records.
    /// </summary>
    /// <param name="students">The collection of student synchronization models.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task BulkSyncStudentsAsync(IEnumerable<StudentSyncModel> students, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a bulk backfill/synchronization of professor profile records.
    /// </summary>
    /// <param name="professors">The collection of professor synchronization models.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task BulkSyncProfessorsAsync(IEnumerable<ProfessorSyncModel> professors, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a bulk backfill/synchronization of project template records.
    /// </summary>
    /// <param name="projects">The collection of project template synchronization models.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task BulkSyncProjectsAsync(IEnumerable<ProjectSyncModel> projects, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a bulk backfill/synchronization of skill records.
    /// </summary>
    /// <param name="skills">The collection of skill synchronization models.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task BulkSyncSkillsAsync(IEnumerable<SkillSyncModel> skills, CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries the AI engine for recommended project template UUIDs based on student context.
    /// </summary>
    /// <param name="query">The project recommendation query parameters.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An ordered list of project template UUIDs sorted by relevance rank.</returns>
    Task<IReadOnlyList<Guid>> GetProjectRecommendationsAsync(GetProjectRecommendationsQueryModel query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries the AI engine for suggested faculty advisor UUIDs based on project blueprint context.
    /// </summary>
    /// <param name="query">The professor suggestion query parameters.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An ordered list of professor UUIDs sorted by relevance rank.</returns>
    Task<IReadOnlyList<Guid>> GetProfessorSuggestionsAsync(GetProfessorSuggestionsQueryModel query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries the AI engine for recommended adjacent skill UUIDs based on student context.
    /// </summary>
    /// <param name="query">The skill recommendation query parameters.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An ordered list of skill UUIDs sorted by relevance rank.</returns>
    Task<IReadOnlyList<Guid>> GetSkillRecommendationsAsync(GetSkillRecommendationsQueryModel query, CancellationToken cancellationToken = default);
}