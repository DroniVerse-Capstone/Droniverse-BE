using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;

namespace Droniverse.Community.Application.IService
{
    public interface IRoundService
    {
        Task<RoundResponseDto> CreateRound(RoundCreateDto request);
        Task<RoundResponseDto> UpdateRound(Guid id, RoundUpdateDto request);
        Task<RoundResponseDto> GetRoundById(Guid id);
        Task<IEnumerable<RoundResponseDto>> GetRoundsByCompetition(Guid competitionId);
        Task<RoundResponseDto> StartRound(Guid id);
        Task<RoundResponseDto> FinishRound(Guid id);
        Task<IEnumerable<RoundLeaderboardEntryDto>> GetRoundLeaderboard(Guid roundId);
    }
}
