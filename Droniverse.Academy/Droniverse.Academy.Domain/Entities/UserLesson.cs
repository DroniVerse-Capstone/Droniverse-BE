using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Domain.Entities;

public class UserLesson
{
    public Guid UserLessonID { get; set; }
    public Lesson Lesson { get; set; }
    public Guid LessonID { get; set; }
    public Guid UserID { get; set; }
    public UserLessonStatus Status { get; set; }
    public float Progress { get; set; }
    public DateTime? LastAccessDate { get; set; }
}
