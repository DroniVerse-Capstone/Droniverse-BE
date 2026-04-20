using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Application.DTO.Response
{
    public class ClubCreationRequestUpdateInfoResponseDto
    {
        public Guid ClubCreationRequestID { get; set; }
        public string NameVN { get; set; } = string.Empty;
        public string NameEN { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsPublic { get; set; }
        public int LimitParticipant { get; set; }
        public int LimitClubManager { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public ClubCreationRequestStatus Status { get; set; }
        public Guid DroneID { get; set; }
        public string? ClubPolicy { get; set; }
        public MediaResponseDto Media { get; set; }
    }
}
