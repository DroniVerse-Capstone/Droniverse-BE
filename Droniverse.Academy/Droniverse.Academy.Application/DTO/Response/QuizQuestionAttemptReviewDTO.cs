namespace Droniverse.Academy.Application.DTO.Response;

public class QuizQuestionAttemptReviewDTO
{
    public UserQuizQuestionAttemptResponseDTO Attempt { get; set; } = null!;
    public QuizQuestionClientViewDTO Question { get; set; } = null!;
}
