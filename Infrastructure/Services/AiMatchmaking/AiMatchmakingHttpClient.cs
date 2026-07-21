using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Common.Models.AiSync;
using Microsoft.Extensions.Logging;

namespace AcademicGateway.Infrastructure.Services.AiMatchmaking;

/// <summary>
/// Infrastructure typed HTTP client for dispatching outbound real-time synchronization 
/// event payloads and bulk backfill batches to the AI Matchmaking Engine.
/// </summary>
public class AiMatchmakingHttpClient : IAiMatchmakingClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AiMatchmakingHttpClient> _logger;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public AiMatchmakingHttpClient(HttpClient httpClient, ILogger<AiMatchmakingHttpClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SyncStudentAsync(StudentSyncModel student, CancellationToken cancellationToken = default)
    {
        await PostAsync("api/v1/sync/student", student, "Student", student.Student.Id, cancellationToken);
    }

    public async Task DeleteStudentAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        await DeleteAsync($"api/v1/sync/student/{studentId}", "Student", studentId, cancellationToken);
    }

    public async Task SyncProfessorAsync(ProfessorSyncModel professor, CancellationToken cancellationToken = default)
    {
        await PostAsync("api/v1/sync/professor", professor, "Professor", professor.Professor.Id, cancellationToken);
    }

    public async Task DeleteProfessorAsync(Guid professorId, CancellationToken cancellationToken = default)
    {
        await DeleteAsync($"api/v1/sync/professor/{professorId}", "Professor", professorId, cancellationToken);
    }

    public async Task SyncProjectAsync(ProjectSyncModel project, CancellationToken cancellationToken = default)
    {
        await PostAsync("api/v1/sync/project", project, "ProjectTemplate", project.ProjectTemplate.Id, cancellationToken);
    }

    public async Task DeleteProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        await DeleteAsync($"api/v1/sync/project/{projectId}", "ProjectTemplate", projectId, cancellationToken);
    }

    public async Task SyncSkillAsync(SkillSyncModel skill, CancellationToken cancellationToken = default)
    {
        await PostAsync("api/v1/sync/skill", skill, "Skill", skill.Skill.Id, cancellationToken);
    }

    public async Task DeleteSkillAsync(Guid skillId, CancellationToken cancellationToken = default)
    {
        await DeleteAsync($"api/v1/sync/skill/{skillId}", "Skill", skillId, cancellationToken);
    }

    public async Task BulkSyncStudentsAsync(IEnumerable<StudentSyncModel> students, CancellationToken cancellationToken = default)
    {
        var command = new BulkSyncStudentCommand { Items = students.ToList() };
        await BulkPostAsync("api/v1/sync/bulk/student", command, "Student", command.Items.Count, cancellationToken);
    }

    public async Task BulkSyncProfessorsAsync(IEnumerable<ProfessorSyncModel> professors, CancellationToken cancellationToken = default)
    {
        var command = new BulkSyncProfessorCommand { Items = professors.ToList() };
        await BulkPostAsync("api/v1/sync/bulk/professor", command, "Professor", command.Items.Count, cancellationToken);
    }

    public async Task BulkSyncProjectsAsync(IEnumerable<ProjectSyncModel> projects, CancellationToken cancellationToken = default)
    {
        var command = new BulkSyncProjectCommand { Items = projects.ToList() };
        await BulkPostAsync("api/v1/sync/bulk/project", command, "ProjectTemplate", command.Items.Count, cancellationToken);
    }

    public async Task BulkSyncSkillsAsync(IEnumerable<SkillSyncModel> skills, CancellationToken cancellationToken = default)
    {
        var command = new BulkSyncSkillCommand { Items = skills.ToList() };
        await BulkPostAsync("api/v1/sync/bulk/skill", command, "Skill", command.Items.Count, cancellationToken);
    }

    private async Task PostAsync<T>(
        string requestUri,
        T payload,
        string entityName,
        Guid entityId,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(requestUri, payload, SerializerOptions, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "Failed to sync {EntityName} ({EntityId}) to AI Matchmaking Engine. Status Code: {StatusCode}, Error: {Error}",
                    entityName,
                    entityId,
                    response.StatusCode,
                    errorContent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "An error occurred while dispatching sync request for {EntityName} ({EntityId}) to AI Matchmaking Engine.",
                entityName,
                entityId);
        }
    }

    private async Task BulkPostAsync<T>(
        string requestUri,
        T payload,
        string entityName,
        int itemCount,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(requestUri, payload, SerializerOptions, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "Failed to bulk sync {ItemCount} {EntityName} records to AI Matchmaking Engine. Status Code: {StatusCode}, Error: {Error}",
                    itemCount,
                    entityName,
                    response.StatusCode,
                    errorContent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "An error occurred while dispatching bulk sync request for {ItemCount} {EntityName} records to AI Matchmaking Engine.",
                itemCount,
                entityName);
        }
    }

    private async Task DeleteAsync(
        string requestUri,
        string entityName,
        Guid entityId,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.DeleteAsync(requestUri, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "Failed to delete {EntityName} ({EntityId}) from AI Matchmaking Engine. Status Code: {StatusCode}, Error: {Error}",
                    entityName,
                    entityId,
                    response.StatusCode,
                    errorContent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "An error occurred while dispatching delete request for {EntityName} ({EntityId}) to AI Matchmaking Engine.",
                entityName,
                entityId);
        }
    }
}