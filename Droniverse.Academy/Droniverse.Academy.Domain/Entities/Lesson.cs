using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Domain.Entities;
public class Lesson
{
    public Guid LessonID { get; set; } //char(36)
    public ICollection<UserLesson> UserLessons { get; set; }
    public Module Module { get; set; }
    public Guid ModuleID { get; set; } //char(36)
    public int OrderIndex { get; set; }
    public LessonType Type { get; set; } //varchar(20)
    public Guid ReferenceID { get; set; } //char(36) // reference to TheoryID or QuizID or LabID based on Type

}
