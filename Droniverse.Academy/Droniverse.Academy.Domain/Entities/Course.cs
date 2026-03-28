using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.Exceptions;

namespace Droniverse.Academy.Domain.Entities;
public class Course
{
    public Guid CourseID { get; set; }

    public ICollection<CourseVersion> CourseVersions { get; set; }
    public CourseVersion? CurrentVersion { get; set; }
    public Guid CreateBy { get; private set; } // reference to UserID
    public DateTime CreateAt { get; private set; }
    public CourseStatus Status { get; private set; } = CourseStatus.DRAFT;
    public Guid? CurrentVersionID { get; set; } // reference to CourseVersionID

    public void SetAuditOnCreate(Guid userId, DateTime now)
    {
        CreateBy = userId;
        CreateAt = now;
    }

    public void Publish()
    {
        if (CurrentVersionID == null)
        {
            throw new DomainException("Cannot publish course without a current version");
        }

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
        if (Status != CourseStatus.DRAFT && Status != CourseStatus.UNPUBLISH)
        {
            throw new DomainException($"Cannot archive course from status {Status}");
        }

        Status = CourseStatus.ARCHIVED;
    }

}
