using System.ComponentModel.DataAnnotations;

namespace Droniverse.Community.Application.DTO.Request
{
    public record CompetitionLevelAddDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "Phải có ít nhất 1 cấp đô")]
        public required List<Guid> LevelIds { get; set; }
    }
}
