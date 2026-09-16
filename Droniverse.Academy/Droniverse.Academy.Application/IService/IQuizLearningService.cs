using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;

namespace Droniverse.Academy.Application.IService;

public interface IQuizLearningService
{
    Task<QuizLearningStateDTO> GetQuizAttemptOrQuizAsync(Guid enrollmentId, Guid quizId);
    Task<QuizAttemptReviewDTO> GetLatestQuizAttemptReviewAsync(Guid enrollmentId, Guid quizId);
    Task<QuizLearningDTO> GetQuizQuestionsForLearningAsync(Guid enrollmentId, Guid quizId);
    Task<SubmitQuizResultDTO> SubmitQuizAsync(Guid enrollmentId, Guid quizId, SubmitQuizRequestDTO request);
}
