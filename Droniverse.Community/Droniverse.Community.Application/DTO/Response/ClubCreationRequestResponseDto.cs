using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Community.Application.DTO.Response;

public class ClubCreationRequestResponseDto
{
    public Guid ClubCreationRequestID { get; set; }
    public string NameVN { get; set; } = string.Empty;
    public string NameEN { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int LimitParticipant { get; set; }
    public int LimitClubManager { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectReason { get; set; }
    public Guid? ClubID { get; set; }
    public Guid RequesterID { get; set; }
    public Guid? ApproverID { get; set; }
    public string? ApproverName { get; set; }
    public string? ApproverEmail { get; set; }
    public string? RequesterName { get; set; }
    public string? RequesterEmail { get; set; }
    public ClubCreationRequestStatus Status { get; set; }
    public MediaResponseDto Media { get; set; }
    public DroneResponseDto Drone { get; set; }
}
