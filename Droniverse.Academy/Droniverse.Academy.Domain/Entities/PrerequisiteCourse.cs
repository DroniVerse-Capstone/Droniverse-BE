namespace Droniverse.Academy.Domain.Entities;

public class PrerequisiteCourse
{
    public Guid CourseID { get; set; }
    public Guid PrerequisiteCourseID { get; set; }

    public Course Course { get; set; }
    public Course RequiredCourse { get; set; }
}
