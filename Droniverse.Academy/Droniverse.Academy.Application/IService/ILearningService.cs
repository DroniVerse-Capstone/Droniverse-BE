using Droniverse.Academy.Application.DTO.Response;

namespace Droniverse.Academy.Application.IService;

public interface ILearningService
{
    Task<LearningPathDTO> GetMyLearningPathAsync(Guid enrollmentId);
    Task<UserLessonResponseDTO> CreateUserLessonAsync(Guid enrollmentId, Guid lessonId);
    Task<bool> CheckUserLessonExistsAsync(Guid enrollmentId, Guid lessonId);
    Task<UserModuleResponseDTO> GetOrCreateUserModuleAsync(Guid enrollmentId, Guid moduleId);
    Task ValidateLessonAccessAsync(Guid enrollmentId, Guid lessonId);
    Task<CompleteLessonResultDTO> CompleteLessonAsync(Guid enrollmentId, Guid lessonId);
}
