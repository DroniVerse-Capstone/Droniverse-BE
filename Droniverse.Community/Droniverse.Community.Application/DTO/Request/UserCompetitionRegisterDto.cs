using System.ComponentModel.DataAnnotations;

namespace Droniverse.Community.Application.DTO.Request
{
    public class UserCompetitionRegisterDto
    {
        [Required]
        public Guid CompetitionID { get; set; }
    }
}
