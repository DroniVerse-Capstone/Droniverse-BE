using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Application.DTO.Response
{
    public class ClubAttemptRequestUpdateStatusResponseDto
    {
        public Guid ClubRequestID { get; set; }
        public Guid RequesterID { get; set; }
        public Guid ClubID { get; set; }
        public string ClubNameVN { get; set; }
        public string ClubNameEN { get; set; }
        public ClubAttemptRequestStatus Status { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public Guid? ParticipationID { get; set; } // If approved
    }
}
