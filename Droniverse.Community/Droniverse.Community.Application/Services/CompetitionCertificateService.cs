using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.IRepository;
using Microsoft.EntityFrameworkCore;

namespace Droniverse.Community.Application.Services
{
    public class CompetitionCertificateService : ICompetitionCertificateService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AcademyMicroserviceClient _academyMicroserviceClient;

        public CompetitionCertificateService(
            IUnitOfWork unitOfWork,
            AcademyMicroserviceClient academyMicroserviceClient)
        {
            _unitOfWork = unitOfWork;
            _academyMicroserviceClient = academyMicroserviceClient;
        }

        public async Task<CompetitionCertificateResponseDto> AddCertificateToCompetition(
            Guid competitionId,
            CompetitionCertificateAddDto request)
        {
            var competition = await _unitOfWork.Competitions.GetByCondition(
                c => c.CompetitionID == competitionId,
                q => q.Include(c => c.CompetitionCertificates)
            );

            if (competition == null)
                throw new KeyNotFoundException($"Không tìm thấy cuộc thi với ID [{competitionId}].");

            // Kiểm tra certificate có tồn tại trong hệ thống Academy
            var certificateExists = await _academyMicroserviceClient.IsCertificateExist(request.CertificateID);
            if (!certificateExists)
                throw new KeyNotFoundException($"Không tìm thấy certificate với ID [{request.CertificateID}] trong hệ thống Academy.");

            // Thêm certificate thông qua domain method
            var competitionCertificate = competition.AddCertificate(request.CertificateID);

            await _unitOfWork.SaveChangeAsync();

            // Lấy thông tin chi tiết certificate từ Academy
            var certificateDetail = await _academyMicroserviceClient.GetCertificateById(request.CertificateID);

            return new CompetitionCertificateResponseDto
            {
                CompetitionID = competitionId,
                CertificateID = request.CertificateID,
                CertificateDetail = certificateDetail != null ? new CertificateDetailDto
                {
                    CertificateID = certificateDetail.CertificateID,
                    CourseVersionID = certificateDetail.CourseVersionID,
                    CertificateName = certificateDetail.CertificateName,
                    ImageUrl = certificateDetail.ImageUrl,
                    LogoCertificate = certificateDetail.LogoCertificate,
                    Description = certificateDetail.Description,
                    Signature = certificateDetail.Signature,
                    AuthorName = certificateDetail.AuthorName,
                    CreateAt = certificateDetail.CreateAt,
                    CreateBy = certificateDetail.CreateBy,
                    UpdateBy = certificateDetail.UpdateBy,
                    UpdateAt = certificateDetail.UpdateAt
                } : null
            };
        }

        public async Task<IEnumerable<CompetitionCertificateResponseDto>> GetCertificatesByCompetition(Guid competitionId)
        {
            var competition = await _unitOfWork.Competitions.GetByCondition(
                c => c.CompetitionID == competitionId,
                q => q.Include(c => c.CompetitionCertificates)
            );

            if (competition == null)
                throw new KeyNotFoundException($"Không tìm thấy cuộc thi với ID [{competitionId}].");

            if (!competition.CompetitionCertificates.Any())
                return Enumerable.Empty<CompetitionCertificateResponseDto>();

            // Lấy danh sách CertificateID
            var certificateIds = competition.CompetitionCertificates
                .Select(cc => cc.CertificateID)
                .ToList();

            // Gọi API bulk để lấy thông tin chi tiết certificate
            var certificateDetails = await _academyMicroserviceClient.GetCertificatesBulk(certificateIds);
            var certificateDict = certificateDetails.ToDictionary(c => c.CertificateID);

            return competition.CompetitionCertificates.Select(cc =>
            {
                certificateDict.TryGetValue(cc.CertificateID, out var detail);

                return new CompetitionCertificateResponseDto
                {
                    CompetitionID = cc.CompetitionID,
                    CertificateID = cc.CertificateID,
                    CertificateDetail = detail != null ? new CertificateDetailDto
                    {
                        CertificateID = detail.CertificateID,
                        CourseVersionID = detail.CourseVersionID,
                        CertificateName = detail.CertificateName,
                        ImageUrl = detail.ImageUrl,
                        LogoCertificate = detail.LogoCertificate,
                        Description = detail.Description,
                        Signature = detail.Signature,
                        AuthorName = detail.AuthorName,
                        CreateAt = detail.CreateAt,
                        CreateBy = detail.CreateBy,
                        UpdateBy = detail.UpdateBy,
                        UpdateAt = detail.UpdateAt
                    } : null
                };
            });
        }

        public async Task<bool> RemoveCertificateFromCompetition(Guid competitionId, Guid certificateId)
        {
            var competition = await _unitOfWork.Competitions.GetByCondition(
                c => c.CompetitionID == competitionId,
                q => q.Include(c => c.CompetitionCertificates)
            );

            if (competition == null)
                throw new KeyNotFoundException($"Không tìm thấy cuộc thi với ID [{competitionId}].");

            competition.RemoveCertificate(certificateId);

            await _unitOfWork.SaveChangeAsync();

            return true;
        }
    }
}