using Droniverse.Community.Application.DTO.Extensions;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Community.Application.IService
{
    public interface ICompetitionService
    {
        Task<CompetitionResponse> CreateCompetition(CompetitionCreationRequest request);
        Task<CompetitionResponse> UpdateCompetition(Guid id, CompetitionUpdateDto request);
        Task<bool> DeleteCompetition(Guid id);
        Task<CompetitionResponse> GetCompetitionById(Guid id);
        Task<PaginationResult<IEnumerable<CompetitionResponse>>> GetAllCompetitionsWithCondition(CompetitionSearchRequest searchRequest);
        Task<IEnumerable<CompetitionResponse>> GetCompetitionsByClub(Guid clubId, CompetitionStatus? status = null);
        Task<PaginationResult<IEnumerable<CompetitionResponse>>> GetHotCompetitionsByClub(Guid clubId, HotCompetitionSearchRequest searchRequest);
        Task<UserCompetitionResponseDto> RegisterForCompetition(Guid competitionId);
        Task<UserCompetitionResponseDto> WithdrawFromCompetition(Guid competitionId);
        Task<IEnumerable<UserCompetitionResponseDto>> GetCompetitionParticipants(Guid competitionId);
        Task<PaginationResult<IEnumerable<LeaderboardEntryDto>>> GetCompetitionLeaderboard(CompetitionLeaderboardSearchRequest request, Guid competitionId);
        Task<CompetitionResponse> UpdateCompetitionStatus(Guid competitionId, CompetitionUpdateStatusDto request);
        Task<RoundResponseDto> GetCurrentRoundByCompetitionID(Guid competitionID);
        Task RefreshHotCompetitionsCacheAsync();

        //Task UpdateCompetitionStatusesAsync();
    }
}
