using Droniverse.Community.Application.DTO.Extensions;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Application.IService
{
    public interface IRoundService
    {
        Task<RoundResponseDto> CreateRound(RoundCreateDto request);
        Task<RoundResponseDto> UpdateRound(Guid id, RoundUpdateDto request);
        Task<RoundResponseDto> UpdateRoundNoLogic(Guid roundId, UpdateRoundNoLogicRequest request);
        Task<RoundResponseDto> GetRoundById(Guid id);
        Task<IEnumerable<RoundResponseDto>> GetRoundsByCompetition(Guid competitionId, RoundStatus? roundStatus = null);
        Task<RoundResponseDto> StartRound(Guid id);
        Task<RoundResponseDto> FinishRound(Guid id);
        Task<RoundJoinResponse> JoinRound(Guid id);

        Task<RoundResponseDto> UpdateRoundStatus(Guid roundId);
        /// <summary>
        /// Lấy danh sách người tham gia của vòng thi theo điều kiện tìm kiếm và phân trang.
        /// </summary>
        Task<PaginationResult<RoundParticipantsResponse>> GetRoundParticipants(Guid roundId, RoundParticipantsSearchRequest request);
        Task<PaginationResult<RoundLeaderBoardResponse>> GetRoundLeaderboard(Guid roundId, RoundLeaderboardSearchRequest request);
        Task<RoundLeaderBoardResponse> CalculateRoundLeaderboard(Guid roundId);
    }
}
