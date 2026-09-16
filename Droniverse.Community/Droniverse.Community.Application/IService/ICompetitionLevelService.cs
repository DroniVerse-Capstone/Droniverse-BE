using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Shared.DTOs;

namespace Droniverse.Community.Application.IService
{
    public interface ICompetitionLevelService
    {
        Task<CompetitionLevelAdditionResponse> AddLevelToCompetition(Guid competitionId, CompetitionLevelAddDto request);
        Task<IEnumerable<SimpleLevelResponse>> GetLevelsByCompetition(Guid competitionId);
        Task<CompetitionLevelDeletionResponse> RemoveLevelsFromCompetition(Guid competitionId, CompetitionLevelRemoveDto request);
    }
}
