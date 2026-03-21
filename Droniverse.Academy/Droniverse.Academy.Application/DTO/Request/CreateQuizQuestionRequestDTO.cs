namespace Droniverse.Academy.Application.DTO.Request;

public class CreateQuizQuestionRequestDTO
{
    public string ContentVN { get; set; } = null!;
    public string ContentEN { get; set; } = null!;
    public string AnswerA { get; set; } = null!;
    public string AnswerB { get; set; } = null!;
    public string AnswerC { get; set; } = null!;
    public string AnswerD { get; set; } = null!;
    public string CorrectAnswer { get; set; } = null!;
    public float Score { get; set; }
}
