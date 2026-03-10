using System.ComponentModel.DataAnnotations;

namespace Droniverse.Community.Application.DTO.Request
{
    public class RoundUpdateDto
    {
        [Required]
        public Guid LabID { get; set; }

        [Required]
        [Range(1, 100)]
        public int RoundNumber { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }
    }
}
