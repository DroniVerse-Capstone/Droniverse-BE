using Droniverse.Community.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Droniverse.Community.Application.DTO.Request
{
    public class ClubCreationRequestUpdateStatusDto
    {

        [Required]
        public ClubCreationRequestStatus Status { get; set; }

        // Optional for rejection
        public string? RejectReason { get; set; }
    }
}
