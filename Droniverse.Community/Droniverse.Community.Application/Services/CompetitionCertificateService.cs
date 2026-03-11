using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Entities;
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

        public async Task<CompetitionCertificatesBulkResponseDto> AddCertificateToCompetition(
            Guid competitionId,
            CompetitionCertificateAddDto request)
        {
            var competition = await _unitOfWork.Competitions.GetByCondition(
                c => c.CompetitionID == competitionId,
                q => q.Include(c => c.CompetitionCertificates)
            );

            if (competition == null)
                throw new KeyNotFoundException($"Không tìm thấy cuộc thi với ID [{competitionId}].");

            // Validate tất cả certificates tồn tại trong Academy system
            //var validationTasks = request.CertificateIDs.Select(certId => 
            //    _academyMicroserviceClient.IsCertificateExist(certId));
            //var validationResults = await Task.WhenAll(validationTasks);

            //var invalidCertificates = request.CertificateIDs
            //    .Where((certId, index) => !validationResults[index])
            //    .ToList();

            //if (invalidCertificates.Any())
            //{
            //    var invalidIds = string.Join(", ", invalidCertificates);
            //    throw new KeyNotFoundException($"Không tìm thấy các certificate với ID: {invalidIds} trong hệ thống Academy.");
            //}

            // Add tất cả certificates
            var addedCertificates = new List<CompetitionCertificate>();
            var skippedCertificates = new List<Guid>();

            foreach (var certificateId in request.CertificateIDs)
            {
                try
                {
                    var competitionCertificate = competition.AddCertificate(certificateId);
                    addedCertificates.Add(competitionCertificate);
                }
                catch (InvalidOperationException)
                {
                    // Certificate đã tồn tại - skip và tiếp tục
                    skippedCertificates.Add(certificateId);
                }
            }

            if (!addedCertificates.Any())
                throw new InvalidOperationException("Tất cả certificates đã được thêm vào cuộc thi trước đó rồi.");

            await _unitOfWork.SaveChangeAsync();

            // Lấy thông tin chi tiết của tất cả certificates đã thêm (bulk API)
            var certificateIds = addedCertificates.Select(ac => ac.CertificateID).ToList();
            var certificateDetails = await _academyMicroserviceClient.GetCertificatesBulk(certificateIds.AsEnumerable());
            //var certificateDict = certificateDetails.ToDictionary(c => c.CertificateID);
            var certificateDict = new Dictionary<Guid, CertificateDetailDto>();

            // Map sang response DTOs
            var certificateResponseList = addedCertificates.Select(ac =>
            {
                certificateDict.TryGetValue(ac.CertificateID, out var detail);

                return new CompetitionCertificateResponseDto
                {
                    CompetitionID = ac.CompetitionID,
                    CertificateID = ac.CertificateID,
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
            }).ToList();

            return new CompetitionCertificatesBulkResponseDto
            {
                CompetitionID = competitionId,
                TotalAdded = addedCertificates.Count,
                Certificates = certificateResponseList
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
            //var certificateDetails = await _academyMicroserviceClient.GetCertificatesBulk(certificateIds);
            //var certificateDict = certificateDetails.ToDictionary(c => c.CertificateID);

            var certificateDict = new Dictionary<Guid, CertificateDetailDto>();


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