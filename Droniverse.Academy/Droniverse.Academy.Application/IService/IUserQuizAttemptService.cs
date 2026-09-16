using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Academy.Application.IService;

public interface IUserQuizAttemptService
{
    Task<UserQuizAttemptResponseDTO> CreateUserQuizAttemptAsync(CreateUserQuizAttemptRequestDTO request);
    Task<PaginationResult<IEnumerable<UserQuizAttemptResponseDTO>>> GetMyQuizAttemptsAsync(int pageIndex = 1, int pageSize = 10, bool? isPassed = null);
    Task<UserQuizAttemptResponseDTO> GetMyQuizAttemptByIdAsync(Guid attemptId);
    Task<UserQuizAttemptResponseDTO> UpdateMyQuizAttemptAsync(Guid attemptId, UpdateUserQuizAttemptRequestDTO request);
    Task DeleteMyQuizAttemptAsync(Guid attemptId);
}
