using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;

namespace Droniverse.Community.Application.IService
{
    public interface IUserRoundService
    {
        Task<UserRoundResponseDto> SubmitSolution(Guid roundId, UserRoundSubmitDto request);
        Task<UserRoundResponseDto> GetUserRoundResult(Guid roundId);
        Task<IEnumerable<UserRoundResponseDto>> GetAllRoundResults(Guid roundId);
    }
}
