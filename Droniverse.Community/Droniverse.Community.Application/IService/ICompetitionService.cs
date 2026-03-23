using Droniverse.Community.Application.DTO.Extensions;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Application.IService
{
    public interface ICompetitionService
    {
        Task<CompetitionResponse> CreateCompetition(CompetitionCreationRequest request);
        Task<CompetitionResponse> UpdateCompetition(Guid id, CompetitionUpdateDto request);
        Task<bool> DeleteCompetition(Guid id);
        Task<CompetitionResponse> GetCompetitionById(Guid id);
        Task<IEnumerable<CompetitionResponse>> GetAllCompetitionsWithCondition(CompetitionSearchRequest searchRequest);
        Task<IEnumerable<CompetitionResponse>> GetCompetitionsByClub(Guid clubId, CompetitionStatus? status = null);
        Task<UserCompetitionResponseDto> RegisterForCompetition(Guid competitionId);
        Task<UserCompetitionResponseDto> WithdrawFromCompetition(Guid competitionId);
        Task<IEnumerable<UserCompetitionResponseDto>> GetCompetitionParticipants(Guid competitionId);
        Task<IEnumerable<LeaderboardEntryDto>> GetCompetitionLeaderboard(Guid competitionId);
        Task<CompetitionResponse> FinishCompetition(Guid competitionId);
        Task UpdateCompetitionStatusesAsync();
    }
}
