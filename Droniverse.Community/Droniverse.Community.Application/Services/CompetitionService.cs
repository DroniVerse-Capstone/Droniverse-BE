using Droniverse.Community.Application.DTO.Extensions;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Shared.Helpers;
using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Droniverse.Community.Application.Services
{
    public class CompetitionService : ICompetitionService
    {
        private const string HotCompetitionCachePrefix = "hot_competitions";

        private static readonly DistributedCacheEntryOptions HotCompetitionCacheOptions = new DistributedCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
            .SetSlidingExpiration(TimeSpan.FromMinutes(2));

        private static readonly ConcurrentDictionary<string, SemaphoreSlim> HotCompetitionLocks = new();

        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IClock _clock;
        private readonly IdentityMicroserviceClient _identityMicroserviceClient;
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<CompetitionService> _logger;

        public CompetitionService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IClock clock,
            IdentityMicroserviceClient identityMicroserviceClient,
            IDistributedCache distributedCache,
            ILogger<CompetitionService> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _clock = clock;
            _identityMicroserviceClient = identityMicroserviceClient;
            _distributedCache = distributedCache;
            _logger = logger;
        }

        public async Task<CompetitionResponse> CreateCompetition(CompetitionCreationRequest request)
        {
            var currentUserId = Guid.Parse(_currentUserService.UserID
                ?? throw new UnauthorizedAccessException("User is not authenticated."));

            var club = await _unitOfWork.Clubs.GetByCondition(c => c.ClubID == request.ClubID);
            if (club == null)
                throw new KeyNotFoundException($"Không tìm thấy club với ID {request.ClubID}.");

            var competition = new Competition(
                request.ClubID,
                request.NameVN,
                request.NameEN,
                request.RuleContent,
                request.VisibleAt.TrimToMinute(),
                request.RegistrationStartDate.TrimToMinute(),
                request.RegistrationEndDate.TrimToMinute(),
                request.StartDate.TrimToMinute(),
                request.EndDate.TrimToMinute(),
                currentUserId,
                _clock.Now,
                request.MaxParticipants,
                request.DescriptionVN,
                request.DescriptionEN
                );

            await _unitOfWork.Competitions.Add(competition);
            await _unitOfWork.SaveChangeAsync();
            await InvalidateHotCompetitionsCache(competition.ClubID);

            return await MapToCompetitionResponse(competition);
        }

        public async Task<CompetitionResponse> UpdateCompetition(Guid id, CompetitionUpdateDto request)
        {
            var currentUserId = Guid.Parse(_currentUserService.UserID
                ?? throw new UnauthorizedAccessException("User is not authenticated."));

            var competition = await _unitOfWork.Competitions.GetByCondition(
                c => c.CompetitionID == id,
                q => q.Include(c => c.Rounds)
                      .Include(c => c.UserCompetitions)
                      .Include(c => c.CompetitionPrizes)
            );

            if (competition == null)
                throw new KeyNotFoundException($"Không tìm thấy cuộc thi với ID {id}.");

            var oldStartDate = competition.StartDate;
            var oldEndDate = competition.EndDate;

            switch (competition.Status)
            {
                case CompetitionStatus.DRAFT:
                    competition.UpdateDraftInformation(
                        request.NameVN,
                        request.NameEN,
                        request.RuleContent,
                        request.VisibleAt,
                        request.RegistrationStartDate,
                        request.RegistrationEndDate,
                        request.StartDate,
                        request.EndDate,
                        currentUserId,
                        _clock.Now,
                        request.MaxParticipants,
                        request.DescriptionVN,
                        request.DescriptionEN
                    );
                    break;

                case CompetitionStatus.PUBLISHED:
                    competition.UpdatePublishedInformation(
                        request.NameVN,
                        request.NameEN,
                        request.DescriptionVN,
                        request.DescriptionEN,
                        request.MaxParticipants,
                        request.RuleContent,
                        currentUserId,
                        _clock.Now
                    );
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Không thể cập nhật cuộc thi ở trạng thái [{competition.Status}].");
            }

            bool timelineChanged = oldStartDate != competition.StartDate || oldEndDate != competition.EndDate;

            if (timelineChanged)
                competition.ValidateAndMarkInvalidRounds();

            await _unitOfWork.SaveChangeAsync();
            await InvalidateHotCompetitionsCache(competition.ClubID);

            return await MapToCompetitionResponse(competition);
        }

        public async Task<bool> DeleteCompetition(Guid id)
        {
            var competition = await _unitOfWork.Competitions.GetByCondition(c => c.CompetitionID == id);
            if (competition == null)
                throw new KeyNotFoundException($"Không tìm thấy cuộc thi với ID [{id}].");

            if (competition.Status != CompetitionStatus.DRAFT)
                throw new InvalidOperationException($"Không thể xóa cuộc thi khi đang ở trạng thái [{competition.Status}]");

            await _unitOfWork.Competitions.Delete(competition);
            await _unitOfWork.SaveChangeAsync();
            await InvalidateHotCompetitionsCache(competition.ClubID);

            return true;
        }

        public async Task<CompetitionResponse> GetCompetitionById(Guid id)
        {
            var competition = await _unitOfWork.Competitions.GetByCondition(
                c => c.CompetitionID == id,
                q => q.AsNoTracking()
            );

            if (competition == null)
                throw new KeyNotFoundException($"Competition with ID {id} not found.");

            return await MapToCompetitionResponse(competition);
        }

        public async Task<PaginationResult<IEnumerable<CompetitionResponse>>> GetAllCompetitionsWithCondition(CompetitionSearchRequest searchRequest)
        {
            int currentPage = searchRequest.CurrentPage <= 0 ? 1 : searchRequest.CurrentPage;
            int pageSize = searchRequest.PageSize <= 0 ? 5 : searchRequest.PageSize;
            int skip = (currentPage - 1) * pageSize;

            var (competitions, totalRecords) = await _unitOfWork.Competitions.GetFilteredCompetitionsAsync(
                searchRequest.CompetitionName,
                searchRequest.Status,
                searchRequest.RegistrationStartDate,
                searchRequest.RegistrationEndDate,
                searchRequest.StartDate,
                searchRequest.EndDate,
                skip,
                pageSize);

            if (totalRecords == 0)
            {
                return new PaginationResult<IEnumerable<CompetitionResponse>>([], 0, currentPage, pageSize);
            }

            var mappedData = (await MapToCompetitionResponses(competitions)).ToList();

            return new PaginationResult<IEnumerable<CompetitionResponse>>(
                mappedData,
                totalRecords,
                currentPage,
                pageSize);
        }

        public async Task<IEnumerable<CompetitionResponse>> GetCompetitionsByClub(Guid clubId, CompetitionStatus? status = null)
        {
            var competitions = await _unitOfWork.Competitions.GetManyByCondition(
                c => c.ClubID == clubId && (!status.HasValue || c.Status == status.Value),
                q => q.Include(c => c.Rounds)
                      .Include(c => c.UserCompetitions)
                      .Include(c => c.CompetitionPrizes)
            );

            return await MapToCompetitionResponses(competitions);
        }

        public async Task<PaginationResult<IEnumerable<CompetitionResponse>>> GetHotCompetitionsByClub(Guid clubId, HotCompetitionSearchRequest searchRequest)
        {
            var currentPage = searchRequest.CurrentPage <= 0 ? 1 : searchRequest.CurrentPage;
            var pageSize = searchRequest.PageSize <= 0 ? 5 : searchRequest.PageSize;

            var ranked = await GetOrBuildHotCompetitionCache(clubId);

            var paged = ranked
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .Select(x => x.Competition)
                .ToList();

            return new PaginationResult<IEnumerable<CompetitionResponse>>(
                paged,
                ranked.Count,
                currentPage,
                pageSize);
        }

        public async Task<UserCompetitionResponseDto> RegisterForCompetition(Guid competitionId)
        {
            var currentUserId = Guid.Parse(_currentUserService.UserID
                ?? throw new UnauthorizedAccessException("Người dùng chưa được xác thực."));

            var competition = await _unitOfWork.Competitions.GetByCondition(
                c => c.CompetitionID == competitionId,
                q => q.Include(c => c.UserCompetitions)
            );

            if (competition == null)
                throw new KeyNotFoundException($"Không tìm thấy cuộc thi với ID [{competitionId}].");

            var userCompetition = competition.RegisterParticipant(currentUserId, _clock.Now);

            await _unitOfWork.UserCompetitions.Add(userCompetition);
            await _unitOfWork.SaveChangeAsync();
            await InvalidateHotCompetitionsCache(competition.ClubID);

            return await MapToUserCompetitionResponse(userCompetition, competition);
        }

        public async Task<UserCompetitionResponseDto> WithdrawFromCompetition(Guid competitionId)
        {
            var currentUserId = Guid.Parse(_currentUserService.UserID
                ?? throw new UnauthorizedAccessException("User is not authenticated."));

            var userCompetition = await _unitOfWork.UserCompetitions.GetByCondition(
                uc => uc.UserID == currentUserId && uc.CompetitionID == competitionId
            );

            if (userCompetition == null)
                throw new KeyNotFoundException("Bạn chưa đăng ký cuộc thi này.");

            var competition = await _unitOfWork.Competitions.GetByCondition(c => c.CompetitionID == competitionId);
            if (competition == null)
                throw new KeyNotFoundException($"Competition with ID {competitionId} not found.");

            if (competition.Status == CompetitionStatus.ONGOING || competition.Status == CompetitionStatus.FINISHED)
                throw new InvalidOperationException("Không thể rút khỏi cuộc thi đã bắt đầu hoặc kết thúc.");

            userCompetition.Withdraw();

            await _unitOfWork.UserCompetitions.Update(userCompetition);
            await _unitOfWork.SaveChangeAsync();
            await InvalidateHotCompetitionsCache(competition.ClubID);

            return await MapToUserCompetitionResponse(userCompetition, competition);
        }

        public async Task<IEnumerable<UserCompetitionResponseDto>> GetCompetitionParticipants(Guid competitionId)
        {
            var competition = await _unitOfWork.Competitions.GetByCondition(c => c.CompetitionID == competitionId);
            if (competition == null)
                throw new KeyNotFoundException($"Competition with ID {competitionId} not found.");

            var participants = await _unitOfWork.UserCompetitions.GetManyByCondition(
                uc => uc.CompetitionID == competitionId && uc.Status == UserCompetitionStatus.ACTIVE
            );

            return await MapToUserCompetitionResponses(participants, competition);
        }

        public async Task<PaginationResult<IEnumerable<LeaderboardEntryDto>>> GetCompetitionLeaderboard(CompetitionLeaderboardSearchRequest request, Guid competitionId)
        {
            var competition = await _unitOfWork.Competitions.GetByCondition(
                c => c.CompetitionID == competitionId,
                query => query.AsNoTracking());

            if (competition == null)
                throw new KeyNotFoundException($"Không tìm thấy cuộc thi với ID [{competitionId}].");

            var participants = await _unitOfWork.UserCompetitions.GetManyByCondition(
                uc => uc.CompetitionID == competitionId && uc.Status == UserCompetitionStatus.ACTIVE,
                q => q.OrderByDescending(uc => uc.Score).ThenBy(uc => uc.UpdatedAt)
            );

            var participantList = participants.ToList();
            if (participantList.Count == 0)
                return Enumerable.Empty<LeaderboardEntryDto>().ToPaginationResult(request);

            var userIds = participantList.Select(uc => uc.UserID).Distinct().ToList();
            var users = await GetUsersBulkSafe(userIds);
            var userDict = users.ToDictionary(u => u.UserId, u => u);

            var leaderboard = participantList.Select((uc, index) =>
            {
                userDict.TryGetValue(uc.UserID, out var user);

                return new LeaderboardEntryDto
                {
                    User = ToSimpleUserResponse(uc.UserID, user),
                    Score = uc.Score,
                    Rank = uc.Rank ?? (index + 1),
                    Status = uc.Status
                };
            });

            return leaderboard.ToPaginationResult(request);
        }

        public async Task<CompetitionResponse> UpdateCompetitionStatus(Guid competitionId, CompetitionUpdateStatusDto request)
        {
            var currentUserId = Guid.Parse(_currentUserService.UserID
                ?? throw new UnauthorizedAccessException("User is not authenticated."));

            if (request.Status == CompetitionStatus.DRAFT)
                throw new InvalidOperationException("Không hỗ trợ cập nhật về trạng thái DRAFT.");

            var include = BuildCompetitionStatusInclude(request.Status);

            var competition = await _unitOfWork.Competitions.GetByCondition(
                c => c.CompetitionID == competitionId,
                include
            );

            if (competition == null)
                throw new KeyNotFoundException($"Không tìm thấy cuộc thi có ID [{competitionId}].");

            competition.UpdateStatus(request.Status, currentUserId, _clock.Now, request.InvalidReason);

            await _unitOfWork.SaveChangeAsync();
            await InvalidateHotCompetitionsCache(competition.ClubID);

            return await MapToCompetitionResponse(competition);
        }

        public async Task RefreshHotCompetitionsCacheAsync()
        {
            var now = _clock.Now;
            var minStartDate = now.AddDays(-30);
            var validStatuses = GetHotStatuses();

            var clubIds = await _unitOfWork.Competitions
                .GetManyByConditionAsQueryable(
                    c => validStatuses.Contains(c.Status) && c.StartDate >= minStartDate,
                    q => q.AsNoTracking())
                .Select(c => c.ClubID)
                .Distinct()
                .ToListAsync();

            foreach (var clubId in clubIds)
            {
                await BuildAndSetHotCompetitionCache(clubId);
            }
        }

        private async Task<List<HotCompetitionCacheItem>> GetOrBuildHotCompetitionCache(Guid clubId)
        {
            var cacheKey = GetHotCompetitionCacheKey(clubId);
            var cached = await TryGetHotCompetitionCache(cacheKey);
            if (cached is not null)
                return cached;

            var lockObj = HotCompetitionLocks.GetOrAdd(cacheKey, _ => new SemaphoreSlim(1, 1));
            await lockObj.WaitAsync();
            try
            {
                cached = await TryGetHotCompetitionCache(cacheKey);
                if (cached is not null)
                    return cached;

                return await BuildAndSetHotCompetitionCache(clubId);
            }
            finally
            {
                lockObj.Release();
            }
        }

        private async Task<List<HotCompetitionCacheItem>> BuildAndSetHotCompetitionCache(Guid clubId)
        {
            var cacheKey = GetHotCompetitionCacheKey(clubId);
            var ranked = await BuildHotCompetitionCacheData(clubId);

            var cacheValue = JsonSerializer.Serialize(ranked);
            await _distributedCache.SetStringAsync(cacheKey, cacheValue, HotCompetitionCacheOptions);

            return ranked;
        }

        private async Task<List<HotCompetitionCacheItem>> BuildHotCompetitionCacheData(Guid clubId)
        {
            var now = _clock.Now;
            var minStartDate = now.AddDays(-30);
            var validStatuses = GetHotStatuses();

            var competitions = await _unitOfWork.Competitions.GetManyByCondition(
                c => c.ClubID == clubId
                     && validStatuses.Contains(c.Status)
                     && c.StartDate >= minStartDate,
                q => q.AsNoTracking());

            var competitionList = competitions.ToList();
            if (competitionList.Count == 0)
                return [];

            var competitionIds = competitionList.Select(c => c.CompetitionID).ToList();
            var aggregateCounts = await _unitOfWork.Competitions.GetAggregateCountsByCompetitionIds(competitionIds);

            var recentJoinDict = await _unitOfWork.UserCompetitions
                .GetManyByConditionAsQueryable(
                    uc => competitionIds.Contains(uc.CompetitionID) && uc.CreatedAt >= now.AddHours(-24),
                    q => q.AsNoTracking())
                .GroupBy(uc => uc.CompetitionID)
                .Select(g => new { CompetitionID = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CompetitionID, x => x.Count);

            var responseMap = await BuildCompetitionResponseMap(competitionList, aggregateCounts);

            return competitionList
                .Select(c =>
                {
                    var counts = aggregateCounts.GetValueOrDefault(
                        c.CompetitionID,
                        (RoundCount: 0, CompetitorCount: 0, PrizeCount: 0));
                    var participants = counts.CompetitorCount;
                    var joinsLast24h = recentJoinDict.GetValueOrDefault(c.CompetitionID, 0);

                    var popularity = Math.Log10(participants + 1d);
                    var recency = CalculateRecencyScore(c.StartDate, now);
                    var statusScore = GetStatusScore(c.Status);
                    var activity = Math.Log10(joinsLast24h + 1d);

                    var hotScore = (0.5 * popularity)
                                   + (0.2 * recency)
                                   + (0.2 * statusScore)
                                   + (0.1 * activity);

                    return new HotCompetitionCacheItem
                    {
                        Competition = responseMap[c.CompetitionID],
                        Score = hotScore,
                        Participants = participants,
                        StartDate = c.StartDate
                    };
                })
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.Participants)
                .ThenByDescending(x => x.StartDate)
                .ToList();
        }

        private async Task<Dictionary<Guid, CompetitionResponse>> BuildCompetitionResponseMap(
            List<Competition> competitionList,
            Dictionary<Guid, (int RoundCount, int CompetitorCount, int PrizeCount)> aggregateCounts)
        {
            var userIds = competitionList
                .Select(c => c.CreatedBy)
                .Union(competitionList.Select(c => c.UpdatedBy).OfType<Guid>())
                .Distinct()
                .ToList();

            var users = await GetUsersBulkSafe(userIds);
            var userDict = users.ToDictionary(u => u.UserId, u => u);

            return competitionList.ToDictionary(
                c => c.CompetitionID,
                c =>
                {
                    userDict.TryGetValue(c.CreatedBy, out var createdByUser);

                    UserResponse? updatedByUser = null;
                    if (c.UpdatedBy.HasValue)
                        userDict.TryGetValue(c.UpdatedBy.Value, out updatedByUser);

                    var counts = aggregateCounts.GetValueOrDefault(
                        c.CompetitionID,
                        (RoundCount: 0, CompetitorCount: 0, PrizeCount: 0));

                    return new CompetitionResponse
                    {
                        CompetitionID = c.CompetitionID,
                        ClubID = c.ClubID,
                        NameVN = c.NameVN,
                        NameEN = c.NameEN,
                        DescriptionVN = c.DescriptionVN,
                        DescriptionEN = c.DescriptionEN,
                        RuleContent = c.RuleContent,
                        MaxParticipants = c.MaxParticipants,
                        VisibleAt = c.VisibleAt,
                        RegistrationStartDate = c.RegistrationStartDate,
                        RegistrationEndDate = c.RegistrationEndDate,
                        StartDate = c.StartDate,
                        EndDate = c.EndDate,
                        Status = c.Status,
                        ResultPublishedAt = c.ResultPublishedAt,
                        CreatedBy = ToSimpleUserResponse(c.CreatedBy, createdByUser),
                        UpdatedBy = c.UpdatedBy.HasValue
                            ? ToSimpleUserResponse(c.UpdatedBy.Value, updatedByUser)
                            : null,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt,
                        InvalidAt = c.InvalidAt,
                        InvalidReason = c.InvalidReason,
                        TotalRounds = counts.RoundCount,
                        TotalCompetitors = counts.CompetitorCount,
                        TotalPrizes = counts.PrizeCount
                    };
                });
        }

        private async Task<List<HotCompetitionCacheItem>?> TryGetHotCompetitionCache(string cacheKey)
        {
            try
            {
                var cache = await _distributedCache.GetStringAsync(cacheKey);
                if (string.IsNullOrWhiteSpace(cache))
                    return null;

                var data = JsonSerializer.Deserialize<List<HotCompetitionCacheItem>>(cache);
                if (data is not null)
                    return data;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không đọc được cache hot competitions key {CacheKey}", cacheKey);
            }

            return null;
        }

        private async Task InvalidateHotCompetitionsCache(Guid clubId)
        {
            var cacheKey = GetHotCompetitionCacheKey(clubId);
            await _distributedCache.RemoveAsync(cacheKey);
        }

        private static string GetHotCompetitionCacheKey(Guid clubId) => $"{HotCompetitionCachePrefix}:{clubId}";

        private static double CalculateRecencyScore(DateTime startDate, DateTime now)
        {
            if (startDate >= now)
                return 1.0;

            var hoursSinceStart = (now - startDate).TotalHours;
            return 1 / (1 + hoursSinceStart);
        }

        private static double GetStatusScore(CompetitionStatus status)
        {
            return status switch
            {
                CompetitionStatus.ONGOING => 1.0,
                CompetitionStatus.REGISTRATION_OPEN => 0.8,
                CompetitionStatus.REGISTRATION_CLOSED => 0.6,
                CompetitionStatus.PUBLISHED => 0.5,
                CompetitionStatus.FINISHED => 0.2,
                CompetitionStatus.CANCELLED => 0,
                CompetitionStatus.INVALID => 0,
                _ => 0
            };
        }

        private static CompetitionStatus[] GetHotStatuses() =>
        [
            CompetitionStatus.PUBLISHED,
            CompetitionStatus.REGISTRATION_OPEN,
            CompetitionStatus.REGISTRATION_CLOSED,
            CompetitionStatus.ONGOING
        ];

        private static Func<IQueryable<Competition>, IQueryable<Competition>>? BuildCompetitionStatusInclude(CompetitionStatus targetStatus)
        {
            return targetStatus switch
            {
                CompetitionStatus.ONGOING => q => q.Include(c => c.Rounds)
                                                    .Include(c => c.UserCompetitions),
                CompetitionStatus.RESULT_PUBLISHED => q => q.Include(c => c.UserPrizes),
                _ => null
            };
        }

        private async Task<CompetitionResponse> MapToCompetitionResponse(Competition competition)
        {
            var competitionIds = new[] { competition.CompetitionID };
            var aggregateCounts = await _unitOfWork.Competitions.GetAggregateCountsByCompetitionIds(competitionIds);
            var counts = aggregateCounts.GetValueOrDefault(
                competition.CompetitionID,
                (RoundCount: 0, CompetitorCount: 0, PrizeCount: 0));

            UserResponse? createdByUser = await GetUserSafe(competition.CreatedBy);
            UserResponse? updatedByUser = null;

            if (competition.UpdatedBy.HasValue)
                updatedByUser = await GetUserSafe(competition.UpdatedBy.Value);

            return new CompetitionResponse
            {
                CompetitionID = competition.CompetitionID,
                ClubID = competition.ClubID,
                NameVN = competition.NameVN,
                NameEN = competition.NameEN,
                DescriptionVN = competition.DescriptionVN,
                DescriptionEN = competition.DescriptionEN,
                RuleContent = competition.RuleContent,
                MaxParticipants = competition.MaxParticipants,
                VisibleAt = competition.VisibleAt,
                RegistrationStartDate = competition.RegistrationStartDate,
                RegistrationEndDate = competition.RegistrationEndDate,
                StartDate = competition.StartDate,
                EndDate = competition.EndDate,
                Status = competition.Status,
                ResultPublishedAt = competition.ResultPublishedAt,
                CreatedBy = ToSimpleUserResponse(competition.CreatedBy, createdByUser),
                UpdatedBy = competition.UpdatedBy.HasValue
                    ? ToSimpleUserResponse(competition.UpdatedBy.Value, updatedByUser)
                    : null,
                CreatedAt = competition.CreatedAt,
                UpdatedAt = competition.UpdatedAt,
                TotalRounds = counts.RoundCount,
                TotalCompetitors = counts.CompetitorCount,
                TotalPrizes = counts.PrizeCount
            };
        }

        private async Task<IEnumerable<CompetitionResponse>> MapToCompetitionResponses(IEnumerable<Competition> competitions)
        {
            var competitionList = competitions.ToList();
            if (competitionList.Count == 0)
                return [];

            var competitionIds = competitionList.Select(c => c.CompetitionID).ToList();
            var userIds = competitionList
                .Select(c => c.CreatedBy)
                .Union(competitionList.Select(c => c.UpdatedBy).OfType<Guid>())
                .Distinct()
                .ToList();

            var usersTask = GetUsersBulkSafe(userIds);
            var aggregateCounts = await _unitOfWork.Competitions.GetAggregateCountsByCompetitionIds(competitionIds);
            var users = await usersTask;

            var userDict = users.ToDictionary(u => u.UserId, u => u);

            return competitionList.Select(competition =>
            {
                userDict.TryGetValue(competition.CreatedBy, out var createdByUser);

                UserResponse? updatedByUser = null;
                if (competition.UpdatedBy.HasValue)
                    userDict.TryGetValue(competition.UpdatedBy.Value, out updatedByUser);

                var counts = aggregateCounts.GetValueOrDefault(
                    competition.CompetitionID,
                    (RoundCount: 0, CompetitorCount: 0, PrizeCount: 0));

                return new CompetitionResponse
                {
                    CompetitionID = competition.CompetitionID,
                    ClubID = competition.ClubID,
                    NameVN = competition.NameVN,
                    NameEN = competition.NameEN,
                    DescriptionVN = competition.DescriptionVN,
                    DescriptionEN = competition.DescriptionEN,
                    RuleContent = competition.RuleContent,
                    MaxParticipants = competition.MaxParticipants,
                    VisibleAt = competition.VisibleAt,
                    RegistrationStartDate = competition.RegistrationStartDate,
                    RegistrationEndDate = competition.RegistrationEndDate,
                    StartDate = competition.StartDate,
                    EndDate = competition.EndDate,
                    Status = competition.Status,
                    ResultPublishedAt = competition.ResultPublishedAt,
                    CreatedBy = ToSimpleUserResponse(competition.CreatedBy, createdByUser),
                    UpdatedBy = competition.UpdatedBy.HasValue
                        ? ToSimpleUserResponse(competition.UpdatedBy.Value, updatedByUser)
                        : null,
                    CreatedAt = competition.CreatedAt,
                    UpdatedAt = competition.UpdatedAt,
                    InvalidAt = competition.InvalidAt,
                    InvalidReason = competition.InvalidReason,
                    TotalRounds = counts.RoundCount,
                    TotalCompetitors = counts.CompetitorCount,
                    TotalPrizes = counts.PrizeCount
                };
            });
        }

        private async Task<UserResponse?> GetUserSafe(Guid userId)
        {
            try
            {
                return await _identityMicroserviceClient.GetUserByUserID(userId);
            }
            catch
            {
                Console.WriteLine("Không lấy được thông tin user từ identity service.");
                return null;
            }
        }

        private async Task<IEnumerable<UserResponse>> GetUsersBulkSafe(IEnumerable<Guid> userIds)
        {
            if (!userIds.Any())
                return [];

            try
            {
                return await _identityMicroserviceClient.GetUsersBulk(userIds);
            }
            catch
            {
                Console.WriteLine("Không lấy được thông tin user từ identity service.");
                return [];
            }
        }

        private class HotCompetitionCacheItem
        {
            public required CompetitionResponse Competition { get; set; }
            public double Score { get; set; }
            public int Participants { get; set; }
            public DateTime StartDate { get; set; }
        }

        private static SimpleUserReponse ToSimpleUserResponse(Guid userId, UserResponse? user)
        {
            return new SimpleUserReponse
            {
                UserId = userId,
                FullName = AppHelper.GetFullName(user) ?? string.Empty,
                Email = user?.Email ?? string.Empty
            };
        }

        private async Task<UserCompetitionResponseDto> MapToUserCompetitionResponse(UserCompetition userCompetition, Competition competition)
        {
            var user = await GetUserSafe(userCompetition.UserID);

            return new UserCompetitionResponseDto
            {
                UserCompetitionID = userCompetition.UserCompetitionID,
                User = ToSimpleUserResponse(userCompetition.UserID, user),
                Competition = new SimpleCompetitionResponse
                {
                    CompetitionID = competition.CompetitionID,
                    NameVN = competition.NameVN,
                    NameEN = competition.NameEN
                },
                Status = userCompetition.Status,
                Score = userCompetition.Score,
                Rank = userCompetition.Rank,
                PrizeID = userCompetition.PrizeID,
                CreatedAt = userCompetition.CreatedAt,
                UpdatedAt = userCompetition.UpdatedAt
            };
        }

        private async Task<IEnumerable<UserCompetitionResponseDto>> MapToUserCompetitionResponses(
            IEnumerable<UserCompetition> userCompetitions,
            Competition competition)
        {
            var list = userCompetitions.ToList();
            if (list.Count == 0)
                return [];

            var userIds = list.Select(x => x.UserID).Distinct().ToList();
            var users = await GetUsersBulkSafe(userIds);
            var userDict = users.ToDictionary(x => x.UserId, x => x);

            return list.Select(uc =>
            {
                userDict.TryGetValue(uc.UserID, out var user);

                return new UserCompetitionResponseDto
                {
                    UserCompetitionID = uc.UserCompetitionID,
                    User = ToSimpleUserResponse(uc.UserID, user),
                    Competition = new SimpleCompetitionResponse
                    {
                        CompetitionID = competition.CompetitionID,
                        NameVN = competition.NameVN,
                        NameEN = competition.NameEN
                    },
                    Status = uc.Status,
                    Score = uc.Score,
                    Rank = uc.Rank,
                    PrizeID = uc.PrizeID,
                    CreatedAt = uc.CreatedAt,
                    UpdatedAt = uc.UpdatedAt
                };
            });
        }
    }
}
