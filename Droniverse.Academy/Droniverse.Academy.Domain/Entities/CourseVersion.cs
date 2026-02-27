using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.Exceptions;

namespace Droniverse.Academy.Domain.Entities;

public class CourseVersion
{
    public Guid CourseVersionID { get; set; }

    public Guid CourseID { get; set; }
    public Course Course { get; set; } = null!;

    public string TitleVN { get; set; } = null!;
    public string TitleEN { get; set; } = null!;

    public string? DescriptionVN { get; set; }
    public string? DescriptionEN { get; set; }

    public string? ContextVN { get; set; }
    public string? ContextEN { get; set; }

    public CourseVersionStatus Status { get; private set; }
        = CourseVersionStatus.DRAFT;

    public int Version { get; set; }

    public string? ImageUrl { get; set; }

    public CourseLevel Level { get; set; }

    public int? EstimatedDuration { get; set; }

    public Guid? UpdateBy { get; private set; }
    public DateTime? UpdateAt { get; private set; }

    /* =========================
       NAVIGATION
       ========================= */

    public ICollection<Module> Modules { get; set; }
        = new List<Module>();

    public ICollection<CourseVersionCategory> CourseVersionCategories { get; set; }
        = new List<CourseVersionCategory>();

    public ICollection<Code> Codes { get; set; }
        = new List<Code>();

    public ICollection<Feedback> Feedbacks { get; set; }
        = new List<Feedback>();

    public ICollection<RequiredDrone> RequiredDrones { get; set; }
        = new List<RequiredDrone>();

    public ICollection<Enrollment> Enrollments { get; set; }
        = new List<Enrollment>();

    public Certificate? Certificate { get; set; }

    /* =========================
       STATE MACHINE
       ========================= */

    // Draft → Active
    public void Activate(Guid userId, DateTime now)
    {
        if (Status != CourseVersionStatus.DRAFT)
        {
            throw new DomainException(
                $"Cannot activate version from status {Status}");
        }

        Status = CourseVersionStatus.ACTIVE;
        SetAudit(userId, now);
    }

    // Active → Deprecated
    public void Deprecate(Guid userId, DateTime now)
    {
        if (Status != CourseVersionStatus.ACTIVE)
        {
            throw new DomainException(
                $"Cannot deprecate version from status {Status}");
        }

        Status = CourseVersionStatus.DEPRECATED;
        SetAudit(userId, now);
    }

    // Any → Inactive (trừ khi đã Inactive)
    public void Inactivate(Guid userId, DateTime now)
    {
        if (Status == CourseVersionStatus.INACTIVE)
        {
            throw new DomainException("Course version already inactive");
        }

        Status = CourseVersionStatus.INACTIVE;
        SetAudit(userId, now);
    }

    private void SetAudit(Guid userId, DateTime now)
    {
        UpdateBy = userId;
        UpdateAt = now;
    }
}