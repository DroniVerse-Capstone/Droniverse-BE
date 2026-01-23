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
    public DateTime CreateAt { get; set; }
    public DateTime UpdateAt { get; set; }
    public Guid CreateBy { get; set; } // reference to UserID
    public Guid UpdateBy { get; set; } // reference to UserID
    public ICollection<QuizQuestion> QuizQuestions { get; set; }
    public Lesson Lesson { get; set; }
    public Guid LessonID { get; set; }
}
