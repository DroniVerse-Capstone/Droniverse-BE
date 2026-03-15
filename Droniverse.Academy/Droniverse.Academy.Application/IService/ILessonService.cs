using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;

namespace Droniverse.Academy.Application.IService;

public interface ILessonService
{
    Task<LessonClientViewDTO> CreateLessonAsync(Guid moduleId, CreateLessonRequestDTO request);
    Task<IEnumerable<LessonClientViewDTO>> GetLessonsByModuleAsync(Guid moduleId);
    Task<LessonClientViewDTO> GetLessonDetailAsync(Guid moduleId, Guid lessonId);
    Task<LessonClientViewDTO> UpdateLessonAsync(Guid moduleId, Guid lessonId, UpdateLessonRequestDTO request);
    Task DeleteLessonAsync(Guid moduleId, Guid lessonId);
}
