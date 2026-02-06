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
    public CourseStatus Status { get; private set; } = CourseStatus.DRAFT;

    public void Publish()
    {
        if (Status != CourseStatus.DRAFT &&
            Status != CourseStatus.UNPUBLISH)
        {
            throw new DomainException(
                $"Cannot publish course from status {Status}");
        }

        Status = CourseStatus.PUBLISH;
    }

    public void Unpublish()
    {
        if (Status != CourseStatus.PUBLISH)
        {
            throw new DomainException(
                $"Cannot unpublish course from status {Status}");
        }

        Status = CourseStatus.UNPUBLISH;
    }

    public void Archive()
    {
        if (Status == CourseStatus.ARCHIVED)
        {
            throw new DomainException("Course already archived");
        }

        Status = CourseStatus.ARCHIVED;
    }

}
