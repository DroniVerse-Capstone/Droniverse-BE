using System.ComponentModel.DataAnnotations;

namespace Droniverse.Community.Application.DTO.Request
{
    public class ClubCreationRequestUpdateInfoDto
    {
        [Required]
        [StringLength(255)]
        public string NameVN { get; set; }

        [Required]
        [StringLength(255)]
        public string NameEN { get; set; }

        [Required]
        [StringLength(2000)]
        public string Description { get; set; }

        [Required]
        public bool IsPublic { get; set; }

        [Range(1, 100000)]
        public int LimitParticipant { get; set; }

        [Range(1, 20)]
        public int LimitClubManager { get; set; }

        [Required]
        public string Image { get; set; }

        [Required]
        public List<Guid> CategoryIDs { get; set; } = new();
    }
}
