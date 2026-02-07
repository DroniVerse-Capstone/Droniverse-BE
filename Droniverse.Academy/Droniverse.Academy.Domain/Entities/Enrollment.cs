using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Domain.Entities;

public class Enrollment
{
    public Guid EnrollmentID { get; set; }          // PK
    public Guid CourseVersionID { get; set; }       // FK -> CourseVersion
    public Guid UserID { get; set; }                // FK -> User
    public Guid? ClubID { get; set; }               // FK -> Club (nullable)
    public DateTime EnrollDate { get; set; }
    public DateTime LastAccessDate { get; set; }
    public DateTime ExpireDate { get; set; }
    public float Progress { get; set; }             // 0 - 100
    public EnrollStatus Status { get; set; }        // tinyint

    // Navigation
    public CourseVersion CourseVersion { get; set; }
}
