using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.DTO.Response;

public class UserAssignmentAttemptResponseDTO
{
    public Guid UserAssignmentID { get; set; }
    public Guid AssignmentID { get; set; }
    public Guid EnrollmentID { get; set; }
    public int AttemptNumber { get; set; }
    public Guid MediaID { get; set; }
    public string Description { get; set; } = string.Empty;
    public UserAssignmentStatus Status { get; set; }
    public int? Score { get; set; }
    public string? ReviewComment { get; set; }
    public Guid? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime SubmittedAt { get; set; }
}
