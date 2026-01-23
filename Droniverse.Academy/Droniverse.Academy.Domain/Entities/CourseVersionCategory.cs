namespace Droniverse.Academy.Domain.Entities;
public class CourseVersionCategory
{
    public Guid CategoryID { get; set; } // reference to CategoryID in AcademyService

    public Guid CourseVersionID { get; set; }
    public CourseVersion CourseVersion { get; set; }
}
