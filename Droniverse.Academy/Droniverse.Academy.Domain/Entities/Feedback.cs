namespace Droniverse.Academy.Domain.Entities;
public class Feedback
{
    public Guid FeedbackID { get; set; }
    public CourseVersion CourseVersion { get; set; }
    public Guid CourseVersionID { get; set; }
    public Guid UserID { get; set; } // reference to UserID
    public int Rating { get; set; } // 1 to 5
    public string Content { get; set; } // text
    public DateTime CreatedAt { get; set; }
}
