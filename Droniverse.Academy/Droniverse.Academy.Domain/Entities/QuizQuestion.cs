namespace Droniverse.Academy.Domain.Entities;

public class QuizQuestion
{
    public Guid QuestionID { get; set; } //char(36)

    public string ContentVN { get; set; }
    public string ContentEN { get; set; }
    public string AnswerA { get; set; }
    public string AnswerB { get; set; }
    public string AnswerC { get; set; }
    public string AnswerD { get; set; }
    public string AnswerA_EN { get; set; }
    public string AnswerB_EN { get; set; }
    public string AnswerC_EN { get; set; }
    public string AnswerD_EN { get; set; }
    public string CorrectAnswer { get; set; }
    public float Score { get; set; } //float
    public Quiz Quiz { get; set; }
    public Guid QuizID { get; set; } //char(36)
    public ICollection<QuizQuestionAttempt> QuizQuestionAttempts { get; set; }
}
