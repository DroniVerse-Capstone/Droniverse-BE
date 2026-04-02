using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Academy.Application.IService;

public interface IUserQuizQuestionAttemptService
{
    Task<UserQuizQuestionAttemptResponseDTO> CreateUserQuizQuestionAttemptAsync(CreateUserQuizQuestionAttemptRequestDTO request);
    Task<PaginationResult<IEnumerable<UserQuizQuestionAttemptResponseDTO>>> GetMyQuizQuestionAttemptsAsync(int pageIndex = 1, int pageSize = 10, bool? isCorrect = null);
    Task<UserQuizQuestionAttemptResponseDTO> GetMyQuizQuestionAttemptByIdAsync(Guid attemptAnswerId);
    Task<UserQuizQuestionAttemptResponseDTO> UpdateMyQuizQuestionAttemptAsync(Guid attemptAnswerId, UpdateUserQuizQuestionAttemptRequestDTO request);
    Task DeleteMyQuizQuestionAttemptAsync(Guid attemptAnswerId);
}
