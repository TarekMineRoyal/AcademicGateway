using AcademicGateway.Application.Common.Models.AiSync;
using Application.Common.Models.AiSync;

namespace AcademicGateway.Application.Common.Interfaces;

public interface IAiMatchmakingClient
{
    Task SyncStudentAsync(StudentSyncModel student, CancellationToken cancellationToken = default);
    Task DeleteStudentAsync(Guid studentId, CancellationToken cancellationToken = default);

    Task SyncProfessorAsync(ProfessorSyncModel professor, CancellationToken cancellationToken = default);
    Task DeleteProfessorAsync(Guid professorId, CancellationToken cancellationToken = default);

    Task SyncProjectAsync(ProjectSyncModel project, CancellationToken cancellationToken = default);
    Task DeleteProjectAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task SyncSkillAsync(SkillSyncModel skill, CancellationToken cancellationToken = default);
    Task DeleteSkillAsync(Guid skillId, CancellationToken cancellationToken = default);
}