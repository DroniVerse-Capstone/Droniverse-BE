using Droniverse.Academy.Application.DTO.Response;

namespace Droniverse.Academy.Application.IService;

public interface ILearningService
{
    Task<LearningPathDTO> GetMyLearningPathAsync(Guid enrollmentId);
    Task ValidateLessonAccessAsync(Guid enrollmentId, Guid lessonId);
    Task<CompleteLessonResultDTO> CompleteLessonAsync(Guid enrollmentId, Guid lessonId);
}
