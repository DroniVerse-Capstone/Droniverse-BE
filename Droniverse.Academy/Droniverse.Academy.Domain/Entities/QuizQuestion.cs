using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Domain.Entities;

public class QuizQuestion
{
    public Guid QuestionID { get; set; } //char(36)

    public string ContentVN { get; set; } //nvarchar(255)
    public string ContentEN { get; set; } //nvarchar(255)
    public QuestionType Type { get; set; } //varchar(30)\
    public float Score { get; set; } //float
    public Quiz Quiz { get; set; }
    public Guid QuizID { get; set; } //char(36)
    public ICollection<QuizAnswer> QuizAnswers { get; set; }

    }
