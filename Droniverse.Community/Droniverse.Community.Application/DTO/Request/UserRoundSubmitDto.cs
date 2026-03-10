using System.ComponentModel.DataAnnotations;

namespace Droniverse.Community.Application.DTO.Request
{
    public class UserRoundSubmitDto
    {
        [Required]
        public string Solution { get; set; }
    }
}
