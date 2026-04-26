using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.DTO.Response;

public class UserAssignmentReviewResponseDTO
{
    public Guid UserAssignmentID { get; set; }
    public Guid AssignmentID { get; set; }
    public Guid EnrollmentID { get; set; }
    public int Score { get; set; }
    public bool IsPassed { get; set; }
    public UserAssignmentStatus Status { get; set; }
    public string? ReviewComment { get; set; }
    public Guid ReviewedBy { get; set; }
    public DateTime ReviewedAt { get; set; }
}
