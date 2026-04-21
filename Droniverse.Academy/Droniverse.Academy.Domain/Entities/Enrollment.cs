using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.Exceptions;

namespace Droniverse.Academy.Domain.Entities;

public class Enrollment
{
    public Guid EnrollmentID { get; set; }          // PK
    public Guid CourseID { get; set; }              // FK -> Course
    public Guid CourseVersionID { get; set; }       // FK -> CourseVersion
    public Guid UserID { get; set; }                // FK -> User
    public Guid ClubID { get; set; }               // FK -> Club 
    public DateTime EnrollDate { get; set; }
    public DateTime LastAccessDate { get; set; }
    public DateTime ExpireDate { get; set; }
    public float Progress { get; set; }             // 0 - 100
    public EnrollStatus Status { get; private set; } = EnrollStatus.ACTIVE;        // tinyint

    // Navigation
    public Course Course { get; set; }
    public CourseVersion CourseVersion { get; set; }

    public void SetProgress(float progress)
    {
        if (progress is < 0 or > 100)
            throw new DomainException("Progress phải nằm trong khoảng từ 0 đến 100.");

        Progress = progress;

        if (Status == EnrollStatus.DROPPED || Status == EnrollStatus.LIMITED_ACCESS)
            return;

        Status = Progress >= 100 ? EnrollStatus.COMPLETED : EnrollStatus.ACTIVE;
    }

    public void Activate()
    {
        if (Status != EnrollStatus.LIMITED_ACCESS && Status != EnrollStatus.DROPPED && Status != EnrollStatus.ACTIVE)
            throw new DomainException($"Cannot activate enrollment from status {Status}");

        if (Progress >= 100)
            throw new DomainException("Cannot activate enrollment when progress is 100.");

        Status = EnrollStatus.ACTIVE;
    }

    public void Complete()
    {
        if (Status == EnrollStatus.DROPPED)
            throw new DomainException("Cannot complete enrollment from status DROPPED");

        if (Progress < 100)
            throw new DomainException("Cannot complete enrollment when progress is less than 100.");

        Status = EnrollStatus.COMPLETED;
    }

    public void LimitAccess()
    {
        if (Status == EnrollStatus.DROPPED)
            throw new DomainException("Cannot limit access for dropped enrollment.");

        Status = EnrollStatus.LIMITED_ACCESS;
    }

    public void Drop()
    {
        if (Status == EnrollStatus.COMPLETED)
            throw new DomainException("Cannot drop completed enrollment.");

        Status = EnrollStatus.DROPPED;
    }

    public void TransitionTo(EnrollStatus targetStatus)
    {
        switch (targetStatus)
        {
            case EnrollStatus.ACTIVE:
                Activate();
                break;
            case EnrollStatus.COMPLETED:
                Complete();
                break;
            case EnrollStatus.LIMITED_ACCESS:
                LimitAccess();
                break;
            case EnrollStatus.DROPPED:
                Drop();
                break;
            default:
                throw new DomainException($"Unsupported enrollment status {targetStatus}");
        }
    }
}
