using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Domain.Entities;
public class Course
{
    public Guid CourseID { get; set; }

    public ICollection<CourseVersion> CourseVersions { get; set; }
    public ICollection<Enrollment> Enrollments { get; set; }
    public Certificate Certificate { get; set; }
    public Guid CreateBy { get; set; } // reference to UserID

    public DateTime CreateAt { get; set; }
    public CourseStatus Status { get; set; } = CourseStatus.ACTIVE;
}
