namespace Droniverse.Academy.Domain.Entities;

public class LevelCourseRequirement
{
    public Guid LevelID { get; set; }
    public Guid CourseID { get; set; }

    public Level Level { get; set; }
    public Course Course { get; set; }
}
