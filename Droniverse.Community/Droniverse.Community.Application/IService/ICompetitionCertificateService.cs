using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;

namespace Droniverse.Community.Application.IService
{
    public interface ICompetitionCertificateService
    {
        Task<CompetitionCertificateResponseDto> AddCertificateToCompetition(Guid competitionId, CompetitionCertificateAddDto request);
        Task<IEnumerable<CompetitionCertificateResponseDto>> GetCertificatesByCompetition(Guid competitionId);
        Task<bool> RemoveCertificateFromCompetition(Guid competitionId, Guid certificateId);
    }
}
