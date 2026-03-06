using Droniverse.Community.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Droniverse.Community.Application.DTO.Request
{
    public class ClubAttemptRequestUpdateStatusDto
    {
        [Required]
        public ClubAttemptRequestStatus Status { get; set; }
    }
}
