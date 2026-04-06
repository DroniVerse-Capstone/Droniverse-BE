using Droniverse.Community.Application.DTO.Extensions;
using Droniverse.Community.Application.DTO.Extensions;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Community.Application.IService
{
    public interface IUserRoundService
    {
        Task<SubmitSolutionResponse> SubmitSolution(Guid roundId, UserRoundSubmitDto request);
        Task<UserRoundResponseDto> GetUserRoundResult(Guid roundId);
        /// <summary>
        /// Lấy kết quả vòng thi theo user và round cụ thể.
        /// </summary>
        Task<UserRoundResponseDto> GetRoundResultByUser(Guid userId, Guid roundId);
        Task<IEnumerable<UserRoundResponseDto>> GetAllRoundResults(Guid roundId);
        /// <summary>
        /// Lấy tất cả kết quả thí sinh của một vòng thi theo bộ lọc, sắp xếp và phân trang.
        /// </summary>
        Task<RoundResultsDto> GetRoundResults(Guid roundId, RoundResultAllParicipations request);
        /// <summary>
        /// Lấy danh sách vòng thi mà người dùng hiện tại đã tham gia, có hỗ trợ lọc và phân trang.
        /// </summary>
        Task<PaginationResult<IEnumerable<MyRoundsResultResponse>>> GetMyUserRound(MyRoundSearchRequest request);
    }
}
