using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Academy.Application.DTO.Response;

public class UserAssignmentAttemptResponseDTO
{
    public Guid UserAssignmentID { get; set; }
    public Guid AssignmentID { get; set; }
    public Guid EnrollmentID { get; set; }
    public int AttemptNumber { get; set; }
    public MediaMiniResponse? Media { get; set; }
    public SimpleUserReponse? User { get; set; }
    public string Description { get; set; } = string.Empty;
    public UserAssignmentStatus Status { get; set; }
    public int? Score { get; set; }
    public string? ReviewComment { get; set; }
    public Guid? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime SubmittedAt { get; set; }
}
