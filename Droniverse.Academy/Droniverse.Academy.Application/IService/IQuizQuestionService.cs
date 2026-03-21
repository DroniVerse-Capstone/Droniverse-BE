using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;

namespace Droniverse.Academy.Application.IService;

public interface IQuizQuestionService
{
    Task<QuizQuestionClientViewDTO> CreateQuizQuestionAsync(Guid quizId, CreateQuizQuestionRequestDTO request);
    Task<IEnumerable<QuizQuestionClientViewDTO>> GetQuizQuestionsByQuizIdAsync(Guid quizId);
    Task<QuizQuestionClientViewDTO> GetQuizQuestionByIdAsync(Guid quizId, Guid questionId);
    Task<QuizQuestionClientViewDTO> UpdateQuizQuestionAsync(Guid quizId, Guid questionId, UpdateQuizQuestionRequestDTO request);
    Task DeleteQuizQuestionAsync(Guid quizId, Guid questionId);
}
