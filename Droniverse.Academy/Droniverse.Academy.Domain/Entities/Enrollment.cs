using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Domain.Entities;
public class Enrollment
{
    public Guid EnrollmentID { get; set; }

    public Course Course { get; set; }
    public Guid CourseID { get; set; }
    public CourseVersion CourseVersion { get; set; }
    public Guid CourseVersionID { get; set; }
    public Guid UserID { get; set; } // reference to UserID
    public Guid? ClubID { get; set; }

    public DateTime EnrollDate { get; set; }
    public DateTime LastAccessDate { get; set; }
    public float Progress { get; set; } = 0;
    public EnrollStatus Status { get; set; } = EnrollStatus.ACTIVE;
    public bool IsCompleted { get; set; } = false;
    public DateTime ExpireDate { get; set; }
    }
