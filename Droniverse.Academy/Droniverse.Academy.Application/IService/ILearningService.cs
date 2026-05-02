using Droniverse.Academy.Application.DTO.Response;

namespace Droniverse.Academy.Application.IService;

public interface ILearningService
{
    Task<LearningPathDTO> GetMyLearningPathAsync(Guid enrollmentId);
    Task<LearningPathDTO> GetLearningPathAsync(Guid enrollmentId);
    Task<IEnumerable<IncompleteVRLessonResponseDTO>> GetListVRsAsync();
    Task<UserLessonResponseDTO> CreateUserLessonAsync(Guid enrollmentId, Guid lessonId);
    Task<bool> CheckUserLessonExistsAsync(Guid enrollmentId, Guid lessonId);
    Task<UserModuleResponseDTO> GetOrCreateUserModuleAsync(Guid enrollmentId, Guid moduleId);
    Task ValidateLessonAccessAsync(Guid enrollmentId, Guid lessonId);
    Task<CompleteLessonResultDTO> CompleteLessonAsync(Guid enrollmentId, Guid lessonId);
    Task<CompleteLessonResultDTO> CompleteLessonBySimulatorSubmitAsync(Guid enrollmentId, Guid lessonId);
    Task<CompleteLessonResultDTO> CompleteLessonByAssessmentAsync(Guid enrollmentId, Guid lessonId);
}
