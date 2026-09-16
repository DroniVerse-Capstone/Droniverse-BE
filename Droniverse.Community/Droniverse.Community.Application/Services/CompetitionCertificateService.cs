using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Droniverse.Community.Application.Services
{
    public class CompetitionCertificateService : ICompetitionCertificateService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AcademyMicroserviceClient _academyMicroserviceClient;
        private readonly ILogger<CompetitionCertificateService> _logger;

        public CompetitionCertificateService(
            IUnitOfWork unitOfWork,
            AcademyMicroserviceClient academyMicroserviceClient,
            ILogger<CompetitionCertificateService> logger)
        {
            _unitOfWork = unitOfWork;
            _academyMicroserviceClient = academyMicroserviceClient;
            _logger = logger;
        }

        public async Task<CompetitionCertificateAdditionResponse> AddCertificateToCompetition(
            Guid competitionId,
            CompetitionCertificateAddDto request)
        {
            if (request == null || request.CertificateIDs == null || request.CertificateIDs.Count == 0)
                throw new ArgumentException("Danh sách chứng chỉ không được để trống.");

            var requestedCertificateIds = request.CertificateIDs
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            if (requestedCertificateIds.Count == 0)
                throw new ArgumentException("Danh sách chứng chỉ không hợp lệ.");

            var competition = await _unitOfWork.Competitions.GetByCondition(
                c => c.CompetitionID == competitionId,
                q => q.Include(c => c.CompetitionCertificates)
            );

            if (competition == null)
                throw new KeyNotFoundException($"Không tìm thấy cuộc thi với ID [{competitionId}].");

            var certificatesFromAcademy = (await _academyMicroserviceClient.GetCertificatesBulk(requestedCertificateIds))
                .ToList();

            var academyCertificateDict = certificatesFromAcademy
                .GroupBy(x => x.CertificateID)
                .ToDictionary(g => g.Key, g => g.First());

            var notFoundCertificateIds = requestedCertificateIds
                .Where(id => !academyCertificateDict.ContainsKey(id))
                .ToList();

            if (notFoundCertificateIds.Count > 0)
            {
                var invalidIds = string.Join(", ", notFoundCertificateIds.Select(x => $"[{x}]"));
                throw new KeyNotFoundException($"Không tìm thấy chứng chỉ trong hệ thống với ID: {invalidIds}.");
            }

            var existingCertificateIds = competition.CompetitionCertificates
                .Select(x => x.CertificateID)
                .ToHashSet();

            var certificateIdsToAdd = requestedCertificateIds
                .Where(id => !existingCertificateIds.Contains(id))
                .ToList();

            if (certificateIdsToAdd.Count == 0)
                throw new InvalidOperationException("Tất cả chứng chỉ đã được thêm vào cuộc thi trước đó.");

            foreach (var certificateId in certificateIdsToAdd)
            {
                competition.AddCertificate(certificateId);
            }

            await _unitOfWork.SaveChangeAsync();

            var certificates = certificateIdsToAdd
                .Select(id => academyCertificateDict[id])
                .ToList();

            return new CompetitionCertificateAdditionResponse
            {
                CompetitionID = competitionId,
                AddedTotal = certificates.Count,
                Certificates = certificates
            };
        }



        public async Task<SimpleCertificateResponse> AddSingleCertificateToCompetition(
            Guid competitionId,
            CompetitionCertificateAddDto request)
        {
            if (request == null || request.CertificateIDs == null || request.CertificateIDs.Count == 0)
                throw new ArgumentException("Danh sách chứng chỉ không được để trống.");

            var distinctCertificateIds = request.CertificateIDs
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            if (distinctCertificateIds.Count != 1)
                throw new ArgumentException("Yêu cầu này chỉ hỗ trợ thêm 1 chứng chỉ.");

            var addResult = await AddCertificateToCompetition(
                competitionId,
                new CompetitionCertificateAddDto
                {
                    CertificateIDs = distinctCertificateIds
                });

            return addResult.Certificates[0];
        }

        public async Task<IEnumerable<SimpleCertificateResponse>> GetCertificatesByCompetition(Guid competitionId)
        {
            var competition = await _unitOfWork.Competitions.GetByCondition(
                c => c.CompetitionID == competitionId,
                q => q.Include(c => c.CompetitionCertificates)
            );

            if (competition == null)
                throw new KeyNotFoundException($"Không tìm thấy cuộc thi với ID [{competitionId}].");

            if (!competition.CompetitionCertificates.Any())
                return [];

            var certificateIds = competition.CompetitionCertificates
                .Select(cc => cc.CertificateID)
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            if (certificateIds.Count == 0)
                return [];

            var certificates = (await _academyMicroserviceClient.GetCertificatesBulk(certificateIds))
                .ToList();

            if (certificates.Count == 0)
                return [];

            var certificateDict = certificates
                .GroupBy(c => c.CertificateID)
                .ToDictionary(g => g.Key, g => g.First());

            return certificateIds
                .Where(certificateDict.ContainsKey)
                .Select(id => certificateDict[id])
                .ToList();
        }

 

        public async Task<CompetitionCertificateDeletionResponse> RemoveCertificatesFromCompetition(Guid competitionId, CompetitionCertificateRemoveDto request)
        {
            if (request == null || request.CertificateIDs == null || request.CertificateIDs.Count == 0)
                throw new ArgumentException("Danh sách chứng chỉ cần xóa không được để trống.");

            var certificateIds = request.CertificateIDs
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            if (certificateIds.Count == 0)
                throw new ArgumentException("Danh sách chứng chỉ cần xóa không hợp lệ.");

            var competition = await _unitOfWork.Competitions.GetByCondition(
                c => c.CompetitionID == competitionId,
                q => q.Include(c => c.CompetitionCertificates)
            );

            if (competition == null)
            {
                _logger.LogWarning("Competition not found when removing certificates. CompetitionId: {CompetitionId}", competitionId);
                throw new KeyNotFoundException($"Không tìm thấy cuộc thi với ID [{competitionId}].");
            }

            var existingCertificateIds = competition.CompetitionCertificates
                .Select(x => x.CertificateID)
                .ToHashSet();

            var removableIds = certificateIds
                .Where(existingCertificateIds.Contains)
                .ToList();

            if (removableIds.Count == 0)
            {
                _logger.LogWarning(
                    "No certificates were removed from competition. CompetitionId: {CompetitionId}, RequestedCount: {RequestedCount}",
                    competitionId,
                    certificateIds.Count);
                throw new KeyNotFoundException("Không có chứng chỉ tồn tại trong cuộc thi để xóa.");
            }

            foreach (var certificateId in removableIds)
            {
                competition.RemoveCertificate(certificateId);
            }

            await _unitOfWork.SaveChangeAsync();

            var remainingCertificateIds = competition.CompetitionCertificates
                .Select(x => x.CertificateID)
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            var remainingCertificates = remainingCertificateIds.Count == 0
                ? []
                : (await _academyMicroserviceClient.GetCertificatesBulk(remainingCertificateIds)).ToList();

            _logger.LogInformation(
                "Removed certificates from competition successfully. CompetitionId: {CompetitionId}, RemovedCount: {RemovedCount}",
                competitionId,
                removableIds.Count);

            return new CompetitionCertificateDeletionResponse
            {
                CompetitionID = competitionId,
                DeletedTotal = removableIds.Count,
                RemainingCertificates = remainingCertificates
            };
        }

        
    }
}