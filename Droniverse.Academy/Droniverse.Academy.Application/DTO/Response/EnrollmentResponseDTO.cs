using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.DTO.Response;

public class EnrollmentResponseDTO
{
    public Guid EnrollmentID { get; set; }
    public Guid CourseID { get; set; }
    public Guid CourseVersionID { get; set; }
    public Guid UserID { get; set; }
    public Guid? ClubID { get; set; }
    public DateTime EnrollDate { get; set; }
    public DateTime LastAccessDate { get; set; }
    public DateTime ExpireDate { get; set; }
    public float Progress { get; set; }
    public EnrollStatus Status { get; set; }
}
