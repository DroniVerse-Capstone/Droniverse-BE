namespace Droniverse.Academy.Domain.Entities;
public class UserAttempt
{
    public Guid AttemptID { get; set; } //char(36)
    public Guid UserID { get; set; } //char(36) // reference to User
    public bool IsCompleted { get; set; } //bit
    public int AttemptTime { get; set; } //int
    public Lesson Lesson { get; set; }
    public Guid LessonID { get; set; } //char(36)

}
