using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace Droniverse.Community.Application.Services
{
    public class CompetitionLevelService : ICompetitionLevelService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AcademyMicroserviceClient _academyMicroserviceClient;
        private readonly ILogger<CompetitionLevelService> _logger;
        public CompetitionLevelService(IUnitOfWork unitOfWork, AcademyMicroserviceClient academyMicroserviceClient, ILogger<CompetitionLevelService> logger)
        {
            _unitOfWork = unitOfWork;
            _academyMicroserviceClient = academyMicroserviceClient;
            _logger = logger;
        }

        public async Task<CompetitionLevelAdditionResponse> AddLevelToCompetition(Guid competitionId, CompetitionLevelAddDto request)
        {
            if (request == null || request.LevelIds == null || request.LevelIds.Count == 0)
                throw new ArgumentException("Danh sách cấp độ không được để trống.");

            var requestedLevelIds = request.LevelIds
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            if (requestedLevelIds.Count == 0)
                throw new ArgumentException("Danh sách cấp độ không hợp lệ.");

            var competition = await _unitOfWork.Competitions.GetByCondition(
                c => c.CompetitionID == competitionId,
                q => q.Include(c => c.CompetitionLevels)
            );

            if (competition == null)
                throw new KeyNotFoundException($"Không tìm thấy cuộc thi với ID [{competitionId}].");

            var levelsFromAcademy = (await _academyMicroserviceClient.GetLevelsBulk(requestedLevelIds))
                .ToList();

            var academyLevelDict = levelsFromAcademy
                .GroupBy(x => x.LevelId)
                .ToDictionary(g => g.Key, g => g.First());

            var notFoundLevelIds = requestedLevelIds
                .Where(id => !academyLevelDict.ContainsKey(id))
                .ToList();

            if (notFoundLevelIds.Count > 0)
            {
                var invalidIds = string.Join(", ", notFoundLevelIds.Select(x => $"[{x}]"));
                throw new KeyNotFoundException($"Không tìm thấy level trong hệ thống với ID: {invalidIds}.");
            }

            var existingLevelIds = competition.CompetitionLevels
                .Select(x => x.LevelID)
                .ToHashSet();

            var levelIdsToAdd = requestedLevelIds
                .Where(id => !existingLevelIds.Contains(id))
                .ToList();

            if (levelIdsToAdd.Count == 0)
                throw new InvalidOperationException("Tất cả level đã được thêm vào cuộc thi trước đó.");

            foreach (var levelId in levelIdsToAdd)
                competition.AddLevel(levelId);

            await _unitOfWork.SaveChangeAsync();

            var levels = levelIdsToAdd
                .Select(id => academyLevelDict[id])
                .ToList();

            return new CompetitionLevelAdditionResponse
            {
                CompetitionID = competitionId,
                AddedTotal = levels.Count,
                Levels = levels
            };
        }

        public async Task<IEnumerable<SimpleLevelResponse>> GetLevelsByCompetition(Guid competitionId)
        {
            var competition = await _unitOfWork.Competitions.GetByCondition(
                c => c.CompetitionID == competitionId,
                q => q.Include(c => c.CompetitionLevels)
            );

            if (competition == null)
                throw new KeyNotFoundException($"Không tìm thấy cuộc thi với ID [{competitionId}].");

            if (!competition.CompetitionLevels.Any())
                return [];

            var levelIds = competition.CompetitionLevels
                .Select(cl => cl.LevelID)
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            if (levelIds.Count == 0)
                return [];

            var levels = (await _academyMicroserviceClient.GetLevelsBulk(levelIds))
                .ToList();

            if (levels.Count == 0)
                return [];

            var levelDict = levels
                .GroupBy(l => l.LevelId)
                .ToDictionary(g => g.Key, g => g.First());

            return levelIds
                .Where(levelDict.ContainsKey)
                .Select(id => levelDict[id])
                .ToList();
        }

        public async Task<CompetitionLevelDeletionResponse> RemoveLevelsFromCompetition(Guid competitionId, CompetitionLevelRemoveDto request)
        {
            if (request == null || request.LevelIds == null || request.LevelIds.Count == 0)
                throw new ArgumentException("Danh sách level cần xóa không được để trống.");

            var levelIds = request.LevelIds
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            if (levelIds.Count == 0)
                throw new ArgumentException("Danh sách level cần xóa không hợp lệ.");

            var competition = await _unitOfWork.Competitions.GetByCondition(
                c => c.CompetitionID == competitionId,
                q => q.Include(c => c.CompetitionLevels)
            );

            if (competition == null)
            {
                _logger.LogWarning("Competition not found when removing levels. CompetitionId: {CompetitionId}", competitionId);
                throw new KeyNotFoundException($"Không tìm thấy cuộc thi với ID [{competitionId}].");
            }

            var existingLevelIds = competition.CompetitionLevels
                .Select(x => x.LevelID)
                .ToHashSet();

            var removableIds = levelIds
                .Where(existingLevelIds.Contains)
                .ToList();

            if (removableIds.Count == 0)
            {
                _logger.LogWarning(
                    "No levels were removed from competition. CompetitionId: {CompetitionId}, RequestedCount: {RequestedCount}",
                    competitionId,
                    levelIds.Count);
                throw new KeyNotFoundException("Không có level tồn tại trong cuộc thi để xóa.");
            }

            foreach (var levelId in removableIds)
            {
                competition.RemoveLevel(levelId);
            }

            await _unitOfWork.SaveChangeAsync();

            var remainingLevelIds = competition.CompetitionLevels
                .Select(x => x.LevelID)
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            var remainingLevels = remainingLevelIds.Count == 0
                ? []
                : (await _academyMicroserviceClient.GetLevelsBulk(remainingLevelIds)).ToList();

            _logger.LogInformation(
                "Removed levels from competition successfully. CompetitionId: {CompetitionId}, RemovedCount: {RemovedCount}",
                competitionId,
                removableIds.Count);

            return new CompetitionLevelDeletionResponse
            {
                CompetitionID = competitionId,
                DeletedTotal = removableIds.Count,
                RemainingLevels = remainingLevels
            };
        }
    }
}
