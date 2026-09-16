namespace Droniverse.Academy.Application.DTO.Response;

public class QuizAttemptReviewDTO
{
    public QuizClientViewDTO Quiz { get; set; } = null!;
    public QuizAttemptDTO Attempt { get; set; } = null!;
    public IEnumerable<QuizQuestionAttemptReviewDTO> Questions { get; set; } = [];
}
