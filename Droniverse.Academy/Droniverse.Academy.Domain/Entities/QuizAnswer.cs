namespace Droniverse.Academy.Domain.Entities;
public class QuizAnswer
{
    public Guid AnswerID { get; set; } //char(36)
    public string ContentVN { get; set; } //nvarchar(255)
    public string ContentEN { get; set; } //nvarchar(255)
    public bool IsCorrect { get; set; } //bit
    public QuizQuestion QuizQuestion { get; set; }
    public Guid QuestionID { get; set; } //char(36) 

}
