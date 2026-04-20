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
    public int? EstimatedDuration { get; set; }
    public string? ChangeLog { get; set; }
    public Guid? UpdateBy { get; private set; }
    public DateTime? UpdateAt { get; private set; }

    /* =========================
       NAVIGATION
       ========================= */

    public ICollection<Module> Modules { get; set; }
        = new List<Module>();

    public ICollection<Feedback> Feedbacks { get; set; }
        = new List<Feedback>();

    public ICollection<Enrollment> Enrollments { get; set; }
        = new List<Enrollment>();

    public Certificate? Certificate { get; set; }

    public void SetAuditOnCreate(Guid userId, DateTime now)
    {
        SetAudit(userId, now);
    }

    public void SetAuditOnUpdate(Guid userId, DateTime now)
    {
        SetAudit(userId, now);
    }

    /* =========================
       STATE MACHINE
       ========================= */

    // Draft → Active
    public void Activate(Guid userId, DateTime now)
    {
        // Allow activating from DRAFT or DEPRECATED per requirements
        if (Status != CourseVersionStatus.DRAFT && Status != CourseVersionStatus.DEPRECATED)
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

    // Draft/Deprecated → Inactive
    public void Inactivate(Guid userId, DateTime now)
    {
        if (Status != CourseVersionStatus.DRAFT && Status != CourseVersionStatus.DEPRECATED)
        {
            throw new DomainException($"Cannot inactivate version from status {Status}");
        }

        Status = CourseVersionStatus.INACTIVE;
        SetAudit(userId, now);
    }

    // Update content - only allowed in DRAFT
    public void UpdateContent(
        string titleVN,
        string titleEN,
        string? descriptionVN,
        string? descriptionEN,
        string? contextVN,
        string? contextEN,
        string? imageUrl,
        int? estimatedDuration,
        string? changeLog,
        Guid userId,
        DateTime now)
    {
        if (Status != CourseVersionStatus.DRAFT)
        {
            throw new DomainException("Can only update content when version is in DRAFT status");
        }

        TitleVN = titleVN;
        TitleEN = titleEN;
        DescriptionVN = descriptionVN;
        DescriptionEN = descriptionEN;
        ContextVN = contextVN;
        ContextEN = contextEN;
        ImageUrl = imageUrl;
        EstimatedDuration = estimatedDuration;
        ChangeLog = changeLog;

        SetAuditOnUpdate(userId, now);
    }

    private void SetAudit(Guid userId, DateTime now)
    {
        UpdateBy = userId;
        UpdateAt = now;
    }
}