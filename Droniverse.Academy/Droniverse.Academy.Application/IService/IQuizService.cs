using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;

namespace Droniverse.Academy.Application.IService;

public interface IQuizService
{
    Task<QuizClientViewDTO> CreateQuizAsync(CreateQuizRequestDTO request);
    Task<IEnumerable<QuizClientViewDTO>> GetQuizzesAsync();
    Task<QuizClientViewDTO> GetQuizByIdAsync(Guid quizId);
    Task<QuizClientViewDTO> UpdateQuizAsync(Guid quizId, UpdateQuizRequestDTO request);
    Task DeleteQuizAsync(Guid quizId);
}
