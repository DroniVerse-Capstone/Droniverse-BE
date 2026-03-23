using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Academy.Application.IService;

public interface IUserLessonService
{
    Task<UserLessonResponseDTO> CreateUserLessonAsync(CreateUserLessonRequestDTO request);
    Task<PaginationResult<IEnumerable<UserLessonResponseDTO>>> GetMyUserLessonsAsync(int pageIndex = 1, int pageSize = 10, UserLessonStatus? status = null);
    Task<UserLessonResponseDTO> GetMyUserLessonByIdAsync(Guid userLessonId);
    Task<UserLessonResponseDTO> UpdateMyUserLessonAsync(Guid userLessonId, UpdateUserLessonRequestDTO request);
    Task DeleteMyUserLessonAsync(Guid userLessonId);
}
