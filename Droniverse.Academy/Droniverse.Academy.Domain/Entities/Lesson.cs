using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Domain.Entities;
public class Lesson
{
    public Guid LessonID { get; set; } //char(36)
    public ICollection<UserAttempt> UserAttempts { get; set; }
    public Module Module { get; set; }
    public Guid ModuleID { get; set; } //char(36)
    public LessonType Type { get; set; } //varchar(20)
    public Lab Lab { get; set; }
    public Theory Theory { get; set; }
    public Quiz Quiz { get; set; }
    public Guid ReferenceID { get; set; } //char(36) // reference to TheoryID or QuizID based on Type

}
