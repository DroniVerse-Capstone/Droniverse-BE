using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs;

namespace Droniverse.Community.Application.DTO.Response
{
    public record CompetitionLevelAdditionResponse
    {
        public Guid CompetitionID { get; set; }
        public int AddedTotal { get; set; }
        public required List<SimpleLevelResponse> Levels { get; set; }
    }
}
