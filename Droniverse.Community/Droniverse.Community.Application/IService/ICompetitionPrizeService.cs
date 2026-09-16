using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;

namespace Droniverse.Community.Application.IService
{
    public interface ICompetitionPrizeService
    {
        Task<CompetitionPrizeResponseDto> CreatePrize(Guid competitionId, CompetitionPrizeCreateDto request);
        Task<CompetitionPrizeResponseDto> UpdatePrize(Guid competitionPrizeId, CompetitionPrizeUpdateDto request);
        Task<DeletePrizeResponse> DeletePrize(Guid competitionPrizeId);
        Task<IEnumerable<CompetitionPrizeResponseDto>> GetPrizesByCompetition(Guid competitionId);
    }
}
