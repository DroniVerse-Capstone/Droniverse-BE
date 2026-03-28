using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Academy.Application.IService;

public interface IAdminUserLearningService
{
    Task<PaginationResult<IEnumerable<UserLabResponseDTO>>> GetUserLabsAsync(Guid userId, int pageIndex = 1, int pageSize = 10, bool? isCompleted = null);
    Task<PaginationResult<IEnumerable<UserQuizAttemptResponseDTO>>> GetUserQuizAttemptsAsync(Guid userId, int pageIndex = 1, int pageSize = 10, bool? isPassed = null);
    Task<PaginationResult<IEnumerable<UserQuizQuestionAttemptResponseDTO>>> GetUserQuizQuestionAttemptsAsync(Guid userId, int pageIndex = 1, int pageSize = 10, bool? isCorrect = null);
    Task<PaginationResult<IEnumerable<UserModuleResponseDTO>>> GetUserModulesAsync(Guid userId, int pageIndex = 1, int pageSize = 10, bool? isCompleted = null);
    Task<PaginationResult<IEnumerable<UserLessonResponseDTO>>> GetUserLessonsAsync(Guid userId, int pageIndex = 1, int pageSize = 10, UserLessonStatus? status = null);
    Task<PaginationResult<IEnumerable<EnrollmentResponseDTO>>> GetUserEnrollmentsAsync(Guid userId, int pageIndex = 1, int pageSize = 10, EnrollStatus? status = null);
}
