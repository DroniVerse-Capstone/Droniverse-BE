using Droniverse.Community.Application.DTO.Extensions;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.AppHelpers;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Infrastructure.QueryModels;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Helpers;
using Droniverse.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace Droniverse.Community.Application.Services
{
    public class RoundService : IRoundService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IdentityMicroserviceClient _identityMicroserviceClient;
        private readonly AcademyMicroserviceClient _academyMicroserviceClient;
        private readonly ICurrentUserService _currentUserService;
        private readonly IClock _clock;

        public RoundService(
            IUnitOfWork unitOfWork,
            IdentityMicroserviceClient identityMicroserviceClient,
            AcademyMicroserviceClient academyMicroserviceClient,
            IClock clock,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _identityMicroserviceClient = identityMicroserviceClient;
            _academyMicroserviceClient = academyMicroserviceClient;
            _clock = clock;
            _currentUserService = currentUserService;
        }

        public async Task<RoundResponseDto> CreateRound(RoundCreateDto request)
        {
            var currentUserId = _currentUserService.UserId;
            var now = _clock.Now;

            var competition = await _unitOfWork.Competitions.GetByCondition(c => c.CompetitionID == request.CompetitionID);
            if (competition == null)
                throw new KeyNotFoundException($"Không tìm thấy cuộc thi với ID [{request.CompetitionID}].");

            var existingRounds = await _unitOfWork.Rounds.GetManyByCondition(r => r.CompetitionID == request.CompetitionID);
            var nextRoundNumber = (existingRounds?.Select(r => r.RoundNumber).DefaultIfEmpty(0).Max() ?? 0) + 1;

            await ValidateRoundData(request.CompetitionID, request.VRSimilatorID, request.StartTime, request.EndTime, request.LimitTime, competition, null);

            var round = new Round(
                request.CompetitionID,
                request.VRSimilatorID,
                nextRoundNumber,
                request.StartTime,
                request.EndTime,
                request.LimitTime,
                now,
                currentUserId
            );

            await _unitOfWork.Rounds.Add(round);
            await _unitOfWork.SaveChangeAsync();

            return await GetRoundResponseByRoundId(round.RoundID);
        }

        public async Task<RoundJoinResponse> JoinRound(Guid id)
        {
            var currentUserId = _currentUserService.UserId;
            var now = _clock.Now;

            var round = await _unitOfWork.Rounds.GetRoundForJoinById(id);
            if (round == null)
                throw new KeyNotFoundException($"Không tìm thấy vòng thi với ID [{id}].");

            var isUserInCompetition = await _unitOfWork.UserCompetitions
      .IsUserInCompetitionAsync(round.CompetitionID, currentUserId);

            var isJoined = await _unitOfWork.UserRounds
                .IsUserJoinedRound(currentUserId, id);

            bool isUserPassedPreviousRound = true;

            if (round.RoundNumber > 1)
            {
                var previousRound = await _unitOfWork.Rounds
                    .GetPreviousRoundByCompetition(round.CompetitionID, round.RoundNumber);

                if (previousRound == null)
                    throw new InvalidOperationException("Không tìm thấy vòng trước.");

                isUserPassedPreviousRound = await _unitOfWork.UserRounds
                    .IsUserPassedRound(currentUserId, previousRound.RoundID);
            }

            // validate trước khi được tham gia
            round.ValidateUserCanJoin(
                now,
                isUserInCompetition,
                isJoined,
                isUserPassedPreviousRound
            );

            var userRound = new UserRound(currentUserId, id, now);
            await _unitOfWork.UserRounds.Add(userRound);
            await _unitOfWork.SaveChangeAsync();

            var deadlineAt = userRound.GetDeadline(round.TimeLimit, round.EndTime);

            return new RoundJoinResponse
            {
                UserRoundId = userRound.UserRoundID,
                StartedAt = userRound.StartedAt,
                DurationInMinutes = (int)Math.Ceiling(round.TimeLimit.TotalMinutes),
                DeadlineAt = deadlineAt,
                ServerTime = now,
                Status = userRound.Status,
                RemainingSeconds = userRound.GetRemainingSeconds(round.TimeLimit, now, round.EndTime)
            };
        }

        public async Task<RoundResponseDto> UpdateRound(Guid id, RoundUpdateDto request)
        {
            var currentUserId = _currentUserService.UserId;
            var now = _clock.Now;

            var round = await _unitOfWork.Rounds.GetByCondition(r => r.RoundID == id);
            if (round == null)
                throw new KeyNotFoundException($"Không tìm thấy vòng thi với ID [{id}].");

            var competition = await _unitOfWork.Competitions.GetByCondition(c => c.CompetitionID == round.CompetitionID);
            if (competition == null)
                throw new KeyNotFoundException($"Không tìm thấy cuộc thi với ID [{round.CompetitionID}].");

            await ValidateRoundData(
                round.CompetitionID,
                request.VRSimulatorID,
                request.StartTime,
                request.EndTime,
                request.TimeLimit,
                competition,
                id
            );

            var wasInvalid = round.Status == RoundStatus.ScheduleInvalid;

            round.UpdateInfo(
                request.VRSimulatorID,
                request.StartTime,
                request.EndTime,
                request.TimeLimit,
                now,
                currentUserId
            );

            if (wasInvalid)
            {
                round.RestoreRound(now, currentUserId);
            }

            await _unitOfWork.Rounds.Update(round);
            await _unitOfWork.SaveChangeAsync();

            return await GetRoundResponseByRoundId(round.RoundID);
        }

        public async Task<RoundResponseDto> GetRoundById(Guid id)
        {
            var round = await _unitOfWork.Rounds.GetRoundByRoundID(id);

            if (round == null)
                throw new KeyNotFoundException($"Không tìm thấy vòng thi với ID [{id}].");

            var competition = await _unitOfWork.Competitions.GetByCondition(c => c.CompetitionID == round.CompetitionID);
            return await MapToRoundResponse(round, competition);
        }

        public async Task<IEnumerable<RoundResponseDto>> GetRoundsByCompetition(Guid competitionId, RoundStatus? roundStatus = null)
        {
            var competition = await _unitOfWork.Competitions.GetByCondition(c => c.CompetitionID == competitionId);
            if (competition == null)
                throw new KeyNotFoundException($"Không tìm thấy cuộc thi với ID [{competitionId}].");

            var rounds = await _unitOfWork.Rounds.GetRoundsByCompetitionID(competitionId, roundStatus);
            if (rounds.Count() == 0)
                return [];

            var vrSimulators = await _academyMicroserviceClient.GetVRSimulatorsByIds(rounds.Select(x => x.VRSimulatorID));
            var vrSimulatorById = vrSimulators.ToDictionary(x => x.VRSimulatorId, x => x);
            var competitionPhase = CommunityAppHelpers.GetCurrentCompetitionLifeCycle(competition, _clock.Now);

            return rounds.Select((r, index) =>
            {
                return new RoundResponseDto
                {
                    RoundID = r.RoundID,
                    Competition = BuildSimpleCompetitionResponse(competition),
                    VRSimulator = BuildSimpleVRSimulatorResponse(r.VRSimulatorID, vrSimulatorById),
                    RoundNumber = index + 1,
                    StartTime = r.StartTime,
                    EndTime = r.EndTime,
                    TimeLimit = r.TimeLimit,
                    RoundStatus = r.Status,
                    RoundPhase = CommunityAppHelpers.GetCurrentRoundLifeCycle(r.Status, r.StartTime, r.EndTime, _clock.Now),
                    TotalParticipants = r.TotalParticipants
                };
            });
        }

        public async Task<RoundResponseDto> StartRound(Guid id)
        {
            var currentUserId = _currentUserService.UserId;
            var round = await _unitOfWork.Rounds.GetByCondition(r => r.RoundID == id);

            if (round == null)
                throw new KeyNotFoundException($"Không tìm thấy vòng thi với ID [{id}].");

            round.StartRound(_clock.Now, currentUserId);

            await _unitOfWork.Rounds.Update(round);
            await _unitOfWork.SaveChangeAsync();

            return await GetRoundResponseByRoundId(round.RoundID);
        }

        public async Task<RoundResponseDto> FinishRound(Guid id)
        {
            var currentUserId = _currentUserService.UserId;
            var round = await _unitOfWork.Rounds.GetByCondition(r => r.RoundID == id);

            if (round == null)
                throw new KeyNotFoundException($"Không tìm thấy vòng thi với ID [{id}].");

            round.FinishRound(_clock.Now, currentUserId);

            await _unitOfWork.Rounds.Update(round);
            await _unitOfWork.SaveChangeAsync();

            return await GetRoundResponseByRoundId(round.RoundID);
        }

        /// <summary>
        /// Lấy danh sách người tham gia của một vòng thi theo điều kiện tìm kiếm và phân trang.
        /// </summary>
        public async Task<PaginationResult<RoundParticipantsResponse>> GetRoundParticipants(Guid roundId, RoundParticipantsSearchRequest request)
        {
            var round = await _unitOfWork.Rounds.GetByCondition(r => r.RoundID == roundId);
            if (round == null)
                throw new KeyNotFoundException($"Không tìm thấy vòng thi với ID [{roundId}].");

            if (!round.IsActive(_clock.Now))
                throw new InvalidOperationException("Vòng thi hiện không ở trạng thái đang diễn ra.");

            ValidateRoundParticipantsRequest(request);
            var (currentPage, pageSize, skip) = NormalizePaging(request);

            IReadOnlyCollection<Guid>? searchedUserIds = null;
            var searchName = request.SearchName?.Trim();
            if (!string.IsNullOrWhiteSpace(searchName))
            {
                try
                {
                    searchedUserIds = (await _identityMicroserviceClient.GetUserIdsBySearchName(new UserInfoSearchRequest
                    {
                        SearchName = searchName,
                        SortDirection = request.SortDirection
                    }))
                    .Where(x => x != Guid.Empty)
                    .Distinct()
                    .ToList();
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Không thể tìm kiếm thông tin người dùng trong hệ thống.", ex);
                }

                if (!searchedUserIds.Any())
                    return EmptyRoundParticipantsResult(round, currentPage, pageSize);
            }

            int totalRecords = await _unitOfWork.UserRounds.CountRoundParticipants(
                roundId,
                request.ParticipantStartedFrom,
                request.ParticipantStartedEnd,
                request.ParticipationSubmittedFrom,
                request.ParticipationSubmittedEnd,
                request.ParticipantStatus,
                request.IsPassed,
                searchedUserIds);

            if (totalRecords == 0)
                return EmptyRoundParticipantsResult(round, currentPage, pageSize);

            var participantRows = await _unitOfWork.UserRounds.GetRoundParticipants(
                roundId,
                request.ParticipantStartedFrom,
                request.ParticipantStartedEnd,
                request.ParticipationSubmittedFrom,
                request.ParticipationSubmittedEnd,
                request.ParticipantStatus,
                request.IsPassed,
                searchedUserIds,
                skip,
                pageSize);

            var pageUserIds = participantRows
                .Select(x => x.UserId)
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            var usersById = await GetUsersBulkSafe(pageUserIds);

            var pagedEntries = participantRows
                .Select(x =>
                {
                    usersById.TryGetValue(x.UserId, out var user);

                    return new RoundParticipantsEntryResponse
                    {
                        User = MapParticipantUser(x.UserId, user),
                        StartedAt = x.StartedAt,
                        Status = x.Status,
                        SubmittedAt = x.SubmittedAt,
                        IsPassed = x.IsPassed
                    };
                })
                .ToList();

            return new PaginationResult<RoundParticipantsResponse>(
                new RoundParticipantsResponse
                {
                    RoundID = round.RoundID,
                    RoundNumber = round.RoundNumber,
                    StartTime = round.StartTime,
                    EndTime = round.EndTime,
                    Status = round.Status,
                    participants = pagedEntries
                },
                totalRecords,
                currentPage,
                pageSize);
        }

        public async Task<PaginationResult<RoundLeaderBoardResponse>> GetRoundLeaderboard(Guid roundId, RoundLeaderboardSearchRequest request)
        {
            var round = await _unitOfWork.Rounds.GetByCondition(r => r.RoundID == roundId);
            if (round == null)
                throw new KeyNotFoundException($"Không tìm thấy vòng thi với ID [{roundId}].");

            if (!round.IsSummarized)
            {
                await CalculateRoundLeaderboard(roundId);
            }

            Guid.TryParse(_currentUserService.UserID, out var currentUserId);

            int currentPage = request.CurrentPage <= 0 ? 1 : request.CurrentPage;
            int pageSize = request.PageSize <= 0 ? 5 : request.PageSize;

            var userRounds = (await _unitOfWork.UserRounds.GetManyByCondition(
                ur => ur.RoundID == roundId && ur.Status == UserRoundStatus.Completed,
                q => q.OrderBy(ur => ur.Rank ?? int.MaxValue)
                      .ThenByDescending(ur => ur.Point)
                      .ThenBy(ur => ur.ExecutionTime)
                      .ThenBy(ur => ur.SubmittedAt)
            )).ToList();

            if (userRounds.Count == 0)
            {
                return new PaginationResult<RoundLeaderBoardResponse>(
                    new RoundLeaderBoardResponse
                    {
                        RoundID = roundId,
                        roundEntries = []
                    },
                    0,
                    currentPage,
                    pageSize);
            }

            var rankedUserRounds = userRounds
                .Select((ur, index) => new
                {
                    UserRound = ur,
                    Rank = ur.Rank ?? (index + 1)
                })
                .ToList();

            if (!string.IsNullOrWhiteSpace(request.SearchName))
            {
                IReadOnlyCollection<Guid> searchedUserIds;
                try
                {
                    searchedUserIds = (await _identityMicroserviceClient.GetUserIdsBySearchName(new UserInfoSearchRequest
                    {
                        SearchName = request.SearchName,
                        SortDirection = request.SortDirection
                    }))
                    .Where(x => x != Guid.Empty)
                    .Distinct()
                    .ToList();
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Không thể tìm kiếm thông tin người dùng trong hệ thống.", ex);
                }

                if (!searchedUserIds.Any())
                {
                    return new PaginationResult<RoundLeaderBoardResponse>(
                        new RoundLeaderBoardResponse
                        {
                            RoundID = roundId,
                            roundEntries = []
                        },
                        0,
                        currentPage,
                        pageSize);
                }

                var searchedUserIdsSet = searchedUserIds.ToHashSet();
                var matchedUserIds = rankedUserRounds
                    .Where(x => searchedUserIdsSet.Contains(x.UserRound.UserID))
                    .Select(x => x.UserRound.UserID)
                    .Where(x => x != Guid.Empty)
                    .Distinct()
                    .ToList();

                var searchedUsersById = await GetUsersBulkSafe(matchedUserIds);

                var searchedEntries = rankedUserRounds
                    .Where(x => searchedUserIdsSet.Contains(x.UserRound.UserID))
                    .Select(x =>
                    {
                        searchedUsersById.TryGetValue(x.UserRound.UserID, out var user);

                        return new RoundLeaderboardEntryDto
                        {
                            User = MapParticipantUser(x.UserRound.UserID, user),
                            Point = x.UserRound.Point ?? 0,
                            ExecutionTime = x.UserRound.ExecutionTime ?? TimeSpan.Zero,
                            IsPassed = x.UserRound.IsPassed ?? false,
                            Status = x.UserRound.Status,
                            SubmittedAt = x.UserRound.SubmittedAt,
                            Rank = x.Rank,
                            IsCurrentUser = currentUserId != Guid.Empty && x.UserRound.UserID == currentUserId
                        };
                    })
                    .ToList();

                return new PaginationResult<RoundLeaderBoardResponse>(
                    new RoundLeaderBoardResponse
                    {
                        RoundID = roundId,
                        roundEntries = searchedEntries
                    },
                    searchedEntries.Count,
                    currentPage,
                    pageSize);
            }

            int totalRecords = rankedUserRounds.Count;
            var pagedUserRounds = rankedUserRounds
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var pagedUserIds = pagedUserRounds
                .Select(x => x.UserRound.UserID)
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            Dictionary<Guid, UserResponse> usersById = [];
            if (pagedUserIds.Count > 0)
            {
                try
                {
                    usersById = (await _identityMicroserviceClient.GetUsersBulk(pagedUserIds))
                        .GroupBy(u => u.UserId)
                        .ToDictionary(g => g.Key, g => g.First());
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Không thể lấy thông tin người dùng từ hệ thống định danh.", ex);
                }
            }

            var pagedEntries = pagedUserRounds
                .Select(x =>
                {
                    usersById.TryGetValue(x.UserRound.UserID, out var user);
                    var fullName = AppHelper.GetFullName(user) ?? user?.Username ?? "Không xác định";

                    return new RoundLeaderboardEntryDto
                    {
                        User = new SimpleUserReponse
                        {
                            UserId = x.UserRound.UserID,
                            FullName = fullName,
                            Email = user?.Email ?? string.Empty
                        },
                        Point = x.UserRound.Point ?? 0,
                        ExecutionTime = x.UserRound.ExecutionTime ?? TimeSpan.Zero,
                        IsPassed = x.UserRound.IsPassed ?? false,
                        Status = x.UserRound.Status,
                        SubmittedAt = x.UserRound.SubmittedAt,
                        Rank = x.Rank,
                        IsCurrentUser = currentUserId != Guid.Empty && x.UserRound.UserID == currentUserId
                    };
                })
                .ToList();

            return new PaginationResult<RoundLeaderBoardResponse>(
                new RoundLeaderBoardResponse
                {
                    RoundID = roundId,
                    roundEntries = pagedEntries
                },
                totalRecords,
                currentPage,
                pageSize);
        }

        public async Task<RoundLeaderBoardResponse> CalculateRoundLeaderboard(Guid roundId)
        {
            var round = await _unitOfWork.Rounds.GetByCondition(r => r.RoundID == roundId);
            if (round == null)
                throw new KeyNotFoundException($"Không tìm thấy vòng thi với ID [{roundId}].");

            Guid.TryParse(_currentUserService.UserID, out var currentUserId);

            var completedUserRounds = (await _unitOfWork.UserRounds.GetManyByCondition(
                ur => ur.RoundID == roundId && ur.Status == UserRoundStatus.Completed,
                q => q.OrderByDescending(ur => ur.Point)
                      .ThenBy(ur => ur.ExecutionTime)
                      .ThenBy(ur => ur.SubmittedAt)
            )).ToList();

            if (completedUserRounds.Count == 0)
            {
                return new RoundLeaderBoardResponse
                {
                    RoundID = roundId,
                    roundEntries = []
                };
            }

            var rankedUserRounds = completedUserRounds
                .Select((ur, index) => new { UserRound = ur, Rank = index + 1 })
                .ToList();

            foreach (var item in rankedUserRounds)
            {
                item.UserRound.SetRank(item.Rank);
                await _unitOfWork.UserRounds.Update(item.UserRound);
            }

            if (round.Status == RoundStatus.Valid && _clock.Now >= round.EndTime && !round.IsSummarized)
            {
                Guid? updatedBy = _currentUserService.IsAuthenticated ? _currentUserService.UserId : null;
                round.MarkSummarized(_clock.Now, updatedBy);
                await _unitOfWork.Rounds.Update(round);
            }

            await _unitOfWork.SaveChangeAsync();

            var userIds = rankedUserRounds
                .Select(x => x.UserRound.UserID)
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            Dictionary<Guid, UserResponse> usersById = [];
            if (userIds.Count > 0)
            {
                try
                {
                    usersById = (await _identityMicroserviceClient.GetUsersBulk(userIds))
                        .GroupBy(u => u.UserId)
                        .ToDictionary(g => g.Key, g => g.First());
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Không thể lấy thông tin người dùng từ hệ thống định danh.", ex);
                }
            }

            var entries = rankedUserRounds
                .Select(x =>
                {
                    usersById.TryGetValue(x.UserRound.UserID, out var user);
                    var fullName = AppHelper.GetFullName(user) ?? user?.Username ?? "Không xác định";

                    return new RoundLeaderboardEntryDto
                    {
                        User = new SimpleUserReponse
                        {
                            UserId = x.UserRound.UserID,
                            FullName = fullName,
                            Email = user?.Email ?? string.Empty
                        },
                        Point = x.UserRound.Point ?? 0,
                        ExecutionTime = x.UserRound.ExecutionTime ?? TimeSpan.Zero,
                        IsPassed = x.UserRound.IsPassed ?? false,
                        Status = x.UserRound.Status,
                        SubmittedAt = x.UserRound.SubmittedAt,
                        Rank = x.Rank,
                        IsCurrentUser = currentUserId != Guid.Empty && x.UserRound.UserID == currentUserId
                    };
                })
                .ToList();

            return new RoundLeaderBoardResponse
            {
                RoundID = roundId,
                roundEntries = entries
            };
        }

        public async Task<RoundResponseDto> UpdateRoundStatus(Guid roundId)
        {
            var round = await _unitOfWork.Rounds.GetByCondition(r => r.RoundID == roundId);
            if (round == null)
                throw new KeyNotFoundException("Không tìm thấy vòng thi");

            round.ValidateCanCancel(_clock.Now);

            var currentUserId = _currentUserService.UserId;

            round.CancelRound(_clock.Now, currentUserId);
            await _unitOfWork.SaveChangeAsync();

            return await GetRoundResponseByRoundId(round.RoundID);
        }

        private async Task ValidateRoundData(
            Guid competitionId,
            Guid vrSimulatorId,
            DateTime startTime,
            DateTime endTime,
            TimeSpan timeLimit,
            Competition competition,
            Guid? excludeRoundId = null)
        {
            // Validate StartTime < EndTime
            if (startTime >= endTime)
                throw new InvalidOperationException("Thời gian bắt đầu của vòng thi phải trước thời gian kết thúc.");

            // Validate TimeLimit > 0
            if (timeLimit <= TimeSpan.Zero)
                throw new InvalidOperationException("Thời gian làm bài (TimeLimit) phải lớn hơn 0.");

            // Validate TimeLimit không vượt quá duration round
            var duration = endTime - startTime;
            if (timeLimit > duration)
                throw new InvalidOperationException("Thời gian làm bài không được vượt quá thời gian của vòng thi.");

            // Validate thời gian Round nằm trong thời gian Competition
            if (startTime < competition.StartDate || startTime > competition.EndDate)
                throw new InvalidOperationException($"Thời gian bắt đầu vòng thi phải nằm trong thời gian cuộc thi ({competition.StartDate:yyyy-MM-dd} đến {competition.EndDate:yyyy-MM-dd}).");

            if (endTime < competition.StartDate || endTime > competition.EndDate)
                throw new InvalidOperationException($"Thời gian kết thúc vòng thi phải nằm trong thời gian cuộc thi ({competition.StartDate:yyyy-MM-dd} đến {competition.EndDate:yyyy-MM-dd}).");

            // Lấy danh sách rounds của competition (exclude round hiện tại nếu đang update)
            var existingRounds = await _unitOfWork.Rounds.GetManyByCondition(
                r => r.CompetitionID == competitionId && ((!excludeRoundId.HasValue || r.RoundID != excludeRoundId.Value) && r.Status != RoundStatus.Cancelled)
            );

            // Validate RoundNumber không trùng
            //var roundWithSameNumber = existingRounds.FirstOrDefault(r => r.RoundNumber == roundNumber);
            //if (roundWithSameNumber != null)
            //    throw new InvalidOperationException($"Số thứ tự vòng thi [{roundNumber}] đã tồn tại trong cuộc thi này.");

            // Validate thời gian không trùng với các round khác
            foreach (var existingRnd in existingRounds)
            {
                bool timeOverlap = (startTime >= existingRnd.StartTime && startTime < existingRnd.EndTime) ||
                                   (endTime > existingRnd.StartTime && endTime <= existingRnd.EndTime) ||
                                   (startTime <= existingRnd.StartTime && endTime >= existingRnd.EndTime);

                if (timeOverlap)
                    throw new InvalidOperationException($"Thời gian vòng thi bị trùng với vòng thi khác ({existingRnd.StartTime:yyyy-MM-dd HH:mm} - {existingRnd.EndTime:yyyy-MM-dd HH:mm}).");
            }

            // Validate VRSimulator không trùng với các round khác
            var roundWithSameSimulator = existingRounds.FirstOrDefault(r => r.VRSimilatorID == vrSimulatorId);
            if (roundWithSameSimulator != null)
                throw new InvalidOperationException($"Bài VR mô phỏng này đã được chọn rồi.");

            // Validate VRSimulator tồn tại trong Academy Microservice (chỉ validate nếu là create hoặc VRSimulatorID thay đổi)
            if (!excludeRoundId.HasValue || (excludeRoundId.HasValue && existingRounds.All(r => r.VRSimilatorID != vrSimulatorId)))
                await _academyMicroserviceClient.GetSimpleVRSimulator(vrSimulatorId);

        }

        private SimpleCompetitionResponse BuildSimpleCompetitionResponse(Competition competition)
        {
            if (competition == null) throw new ArgumentNullException("Không tìm thấy dữ liệu cuộc thi về bài VR mô phỏng này!");

            return new SimpleCompetitionResponse
            {
                CompetitionID = competition.CompetitionID,
                NameVN = competition.NameVN,
                NameEN = competition.NameEN
            };
        }

        private async Task<RoundResponseDto> GetRoundResponseByRoundId(Guid roundId)
        {
            var round = await _unitOfWork.Rounds.GetRoundByRoundID(roundId);
            if (round == null)
                throw new KeyNotFoundException($"Không tìm thấy vòng thi với ID [{roundId}].");

            var competition = await _unitOfWork.Competitions.GetByCondition(c => c.CompetitionID == round.CompetitionID);
            return await MapToRoundResponse(round, competition);
        }

        private async Task<RoundResponseDto> MapToRoundResponse(RoundQueryModel round, Competition? competition)
        {
            var vrSimulators = await _academyMicroserviceClient.GetVRSimulatorsByIds([round.VRSimulatorID]);
            var simulatorById = vrSimulators.ToDictionary(x => x.VRSimulatorId, x => x);

            var competitionPhase = competition != null
                ? CommunityAppHelpers.GetCurrentCompetitionLifeCycle(competition, _clock.Now)
                : null;

            return new RoundResponseDto
            {
                RoundID = round.RoundID,
                Competition = new SimpleCompetitionResponse
                {
                    CompetitionID = round.CompetitionID,
                    NameVN = round.NameVN,
                    NameEN = round.NameEN
                },
                VRSimulator = BuildSimpleVRSimulatorResponse(round.VRSimulatorID, simulatorById),
                RoundNumber = round.RoundNumber,
                StartTime = round.StartTime,
                EndTime = round.EndTime,
                TimeLimit = round.TimeLimit,
                RoundStatus = round.Status,
                RoundPhase = CommunityAppHelpers.GetCurrentRoundLifeCycle(round.Status, round.StartTime, round.EndTime, _clock.Now),
                TotalParticipants = round.TotalParticipants
            };
        }

        private static SimpleUserReponse MapParticipantUser(Guid userId, UserResponse? user)
        {
            var fullName = AppHelper.GetFullName(user) ?? user?.Username ?? "Không xác định";

            return new SimpleUserReponse
            {
                UserId = userId,
                FullName = fullName,
                Email = user?.Email ?? string.Empty
            };
        }

        private static void ValidateRoundParticipantsRequest(RoundParticipantsSearchRequest request)
        {
            if (request.ParticipantStartedFrom.HasValue
                && request.ParticipantStartedEnd.HasValue
                && request.ParticipantStartedFrom > request.ParticipantStartedEnd)
            {
                throw new InvalidOperationException("Khoảng thời gian bắt đầu tham gia không hợp lệ.");
            }

            if (request.ParticipationSubmittedFrom.HasValue
                && request.ParticipationSubmittedEnd.HasValue
                && request.ParticipationSubmittedFrom > request.ParticipationSubmittedEnd)
            {
                throw new InvalidOperationException("Khoảng thời gian nộp bài không hợp lệ.");
            }
        }

        private static (int CurrentPage, int PageSize, int Skip) NormalizePaging(SearchRequest request)
        {
            int currentPage = Math.Max(1, request.CurrentPage);
            int pageSize = Math.Max(1, request.PageSize);
            int skip = (currentPage - 1) * pageSize;

            return (currentPage, pageSize, skip);
        }

        private static PaginationResult<RoundParticipantsResponse> EmptyRoundParticipantsResult(Round round, int currentPage, int pageSize)
        {
            return new PaginationResult<RoundParticipantsResponse>(
                new RoundParticipantsResponse
                {
                    RoundID = round.RoundID,
                    RoundNumber = round.RoundNumber,
                    StartTime = round.StartTime,
                    EndTime = round.EndTime,
                    Status = round.Status,
                    participants = []
                },
                0,
                currentPage,
                pageSize);
        }

        private async Task<Dictionary<Guid, UserResponse>> GetUsersBulkSafe(IReadOnlyCollection<Guid> userIds)
        {
            if (userIds.Count == 0)
                return [];

            try
            {
                return (await _identityMicroserviceClient.GetUsersBulk(userIds))
                    .GroupBy(x => x.UserId)
                    .ToDictionary(x => x.Key, x => x.First());
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Không thể lấy thông tin người dùng từ hệ thống định danh.", ex);
            }
        }

        private static SimpleVRSimulatorResponse BuildSimpleVRSimulatorResponse(Guid vrSimulatorId, IReadOnlyDictionary<Guid, SimpleVRSimulatorResponse> vrSimulatorById)
        {
            if (vrSimulatorById.TryGetValue(vrSimulatorId, out var vrSimulator))
                return vrSimulator;

            return new SimpleVRSimulatorResponse
            {
                VRSimulatorId = vrSimulatorId,
                TitleVN = "Unknown Simulation",
                TitleEN = "Unknown Simulation"
            };
        }
    }
}
