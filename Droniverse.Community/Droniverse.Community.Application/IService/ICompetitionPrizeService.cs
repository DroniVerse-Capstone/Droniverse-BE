using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;

namespace Droniverse.Community.Application.IService
{
    public interface ICompetitionPrizeService
    {
        Task<CompetitionPrizeResponseDto> CreatePrize(CompetitionPrizeCreateDto request);
        Task<CompetitionPrizeResponseDto> UpdatePrize(Guid id, CompetitionPrizeUpdateDto request);
        Task<bool> DeletePrize(Guid id);
        Task<IEnumerable<CompetitionPrizeResponseDto>> GetPrizesByCompetition(Guid competitionId);
    }
}
