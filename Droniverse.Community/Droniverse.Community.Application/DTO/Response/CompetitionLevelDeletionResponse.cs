using Droniverse.Shared.DTOs;

namespace Droniverse.Community.Application.DTO.Response
{
    public record CompetitionLevelDeletionResponse
    {
        public Guid CompetitionID { get; set; }
        public int DeletedTotal { get; set; }
        public required List<SimpleLevelResponse> RemainingLevels { get; set; }
    }
}
