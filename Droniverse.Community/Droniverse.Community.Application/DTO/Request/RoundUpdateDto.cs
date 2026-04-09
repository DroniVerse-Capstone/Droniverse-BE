using System.ComponentModel.DataAnnotations;

namespace Droniverse.Community.Application.DTO.Request
{
    public class RoundUpdateDto
    {
        [Required]
        public Guid LabID { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }
        [Required]
        public TimeSpan TimeLimit { get; set; }
    }
}
