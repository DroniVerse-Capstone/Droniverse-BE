using System.ComponentModel.DataAnnotations;

namespace Droniverse.Community.Application.DTO.Request
{
    public class RoundCreateDto
    {
        [Required]
        public Guid CompetitionID { get; set; }

        [Required]
        public Guid LabID { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }
        [Required]
        public TimeSpan LimitTime { get; set; }
    }
}
