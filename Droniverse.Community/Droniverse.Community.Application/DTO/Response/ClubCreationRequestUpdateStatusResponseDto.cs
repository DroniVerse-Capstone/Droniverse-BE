using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Application.DTO.Response
{
    public class ClubCreationRequestUpdateStatusResponseDto
    {
        public Guid ClubCreationRequestID { get; set; }
        public string NameVN { get; set; }
        public string NameEN { get; set; }
        public ClubCreationRequestStatus Status { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? RejectReason { get; set; }
        public Guid? ClubID { get; set; }
    }
}
