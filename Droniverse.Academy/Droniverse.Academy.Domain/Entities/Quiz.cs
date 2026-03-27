namespace Droniverse.Academy.Domain.Entities;
public class Quiz
{
    public Guid QuizID { get; set; }
    public string TitleVN { get; set; }
    public string TitleEN { get; set; }
    public string DescriptionVN { get; set; }
    public string DescriptionEN { get; set; }
    public int TimeLimit { get; set; } // in minutes
    public float TotalScore { get; set; }
    public float PassScore { get; set; }
    public DateTime CreateAt { get; private set; }
    public DateTime UpdateAt { get; private set; }
    public Guid CreateBy { get; private set; } // reference to UserID
    public Guid UpdateBy { get; private set; } // reference to UserID
    public ICollection<QuizQuestion> QuizQuestions { get; set; }
    public ICollection<QuizAttempt> QuizAttempts { get; set; }

    public void SetAuditOnCreate(Guid userId, DateTime now)
    {
        CreateBy = userId;
        CreateAt = now;
        UpdateBy = userId;
        UpdateAt = now;
    }

    public void SetAuditOnUpdate(Guid userId, DateTime now)
    {
        UpdateBy = userId;
        UpdateAt = now;
    }
}
