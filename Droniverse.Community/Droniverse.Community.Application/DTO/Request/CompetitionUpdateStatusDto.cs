using Droniverse.Community.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Droniverse.Community.Application.DTO.Request
{
    public class CompetitionUpdateStatusDto
    {
        [Required]
        public CompetitionStatus Status { get; set; }
        public CompetitionInvalidReason? InvalidReason { get; set; }
    }
}
