using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Shared.DTOs;

namespace Droniverse.Community.Application.IService
{
    public interface ICompetitionCertificateService
    {
        Task<CompetitionCertificateAdditionResponse> AddCertificateToCompetition(Guid competitionId, CompetitionCertificateAddDto request);
        Task<SimpleCertificateResponse> AddSingleCertificateToCompetition(Guid competitionId, CompetitionCertificateAddDto request);
        Task<IEnumerable<SimpleCertificateResponse>> GetCertificatesByCompetition(Guid competitionId);
        Task<CompetitionCertificateDeletionResponse> RemoveCertificatesFromCompetition(Guid competitionId, CompetitionCertificateRemoveDto request);
    }
}
