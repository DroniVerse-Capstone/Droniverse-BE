using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.DTO.Response;

public class UserAssignmentSubmitResponseDTO
{
    public Guid UserAssignmentID { get; set; }
    public Guid AssignmentID { get; set; }
    public Guid EnrollmentID { get; set; }
    public int AttemptNumber { get; set; }
    public UserAssignmentStatus Status { get; set; }
    public DateTime SubmittedAt { get; set; }
}
