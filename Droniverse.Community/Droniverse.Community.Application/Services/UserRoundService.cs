using Droniverse.Community.Application.DTO.Extensions;
using Droniverse.Community.Application.DTO.Extensions;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Helpers;
using Droniverse.Shared.Services;

namespace Droniverse.Community.Application.Services
{
    public class UserRoundService : IUserRoundService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IdentityMicroserviceClient _identityMicroserviceClient;
        private readonly ICurrentUserService _currentUserService;
        private readonly IClock _clock;

        public UserRoundService(
            IUnitOfWork unitOfWork,
            IdentityMicroserviceClient identityMicroserviceClient,
            ICurrentUserService currentUserService,
            IClock clock)
        {
            _unitOfWork = unitOfWork;
            _identityMicroserviceClient = identityMicroserviceClient;
            _currentUserService = currentUserService;
            _clock = clock;
        }

        public async Task<SubmitSolutionResponse> SubmitSolution(Guid roundId, UserRoundSubmitDto request)
        {
            var currentUserId = _currentUserService.UserId;
            var now = _clock.Now;

            var round = await _unitOfWork.Rounds.GetByCondition(r => r.RoundID == roundId);
            if (round == null)
                throw new KeyNotFoundException($"Không tìm thấy vòng thi với ID [{roundId}].");

            round.ValidateUserCanSubmit(now);

            var userRound = await _unitOfWork.UserRounds.GetByCondition(
                ur => ur.UserID == currentUserId && ur.RoundID == roundId
            );

            if (userRound == null)
                throw new KeyNotFoundException("Bạn chưa tham gia vòng thi này.");

            userRound.Complete(
                request.Solution,
                request.ExecutionTime,
                request.NumberOfSteps,
                request.Point,
                request.IsPassed,
                request.IsSequentialCheckpoints,
                round.TimeLimit,
                now
            );

            await _unitOfWork.UserRounds.Update(userRound);
            await _unitOfWork.SaveChangeAsync();

            return new SubmitSolutionResponse
            {
                UserRoundId = userRound.UserRoundID,
                SubmittedAt = userRound.SubmittedAt ?? userRound.GetEffectiveSubmittedAt(round.TimeLimit, now),
                Status = userRound.Status
            };
        }

        public async Task<UserRoundResponseDto> GetUserRoundResult(Guid roundId)
        {
            var currentUserId = Guid.Parse(_currentUserService.UserID ?? throw new UnauthorizedAccessException("Người dùng chưa được xác thực."));

            var round = await _unitOfWork.Rounds.GetByCondition(r => r.RoundID == roundId);
            if (round == null)
                throw new KeyNotFoundException($"Không tìm thấy vòng thi với ID [{roundId}].");

            var userRound = await _unitOfWork.UserRounds.GetByCondition(
                ur => ur.UserID == currentUserId && ur.RoundID == roundId
            );

            if (userRound == null)
                throw new KeyNotFoundException("Bạn chưa tham gia vòng thi này.");

            return MapToUserRoundResponse(userRound, round);
        }

        /// <summary>
        /// Lấy kết quả vòng thi của một người dùng theo mã vòng thi.
        /// </summary>
        public async Task<UserRoundResponseDto> GetRoundResultByUser(Guid userId, Guid roundId)
        {
            if (userId == Guid.Empty)
                throw new InvalidOperationException("Mã người dùng không hợp lệ.");

            var result = await _unitOfWork.UserRounds.GetRoundResultByUser(userId, roundId);
            if (result == null)
            {
                var roundExists = await _unitOfWork.Rounds.GetByCondition(r => r.RoundID == roundId) != null;
                if (!roundExists)
                    throw new KeyNotFoundException($"Không tìm thấy vòng thi với ID [{roundId}].");

                throw new KeyNotFoundException("Người dùng chưa tham gia vòng thi này.");
            }

            return new UserRoundResponseDto
            {
                UserRoundID = result.UserRoundId,
                Status = result.Status,
                Point = result.Point ?? 0,
                ExecutionTime = result.ExecutionTime ?? TimeSpan.Zero,
                StartedAt = result.StartedAt,
                SubmittedAt = result.SubmittedAt ?? result.StartedAt,
                IsPassed = result.IsPassed ?? false,
                Rank = result.Rank,
                Round = new SimpleRoundResponse
                {
                    RoundId = result.RoundId,
                    RoundNumber = result.RoundNumber,
                    StartTime = result.RoundStartTime,
                    EndTime = result.RoundEndTime,
                    TimeLimit = result.RoundTimeLimit,
                    RoundStatus = result.RoundStatus
                }
            };
        }

        /// <summary>
        /// Lấy danh sách vòng thi mà người dùng hiện tại đã tham gia theo điều kiện lọc và phân trang.
        /// </summary>
        public async Task<PaginationResult<IEnumerable<MyRoundsResultResponse>>> GetMyUserRound(MyRoundSearchRequest request)
        {
            var currentUserId = _currentUserService.UserId;

            int currentPage = request.CurrentPage;
            int pageSize = request.PageSize;
            int skip = (currentPage - 1) * pageSize;

            int totalRecords = await _unitOfWork.UserRounds.CountMyRounds(
                currentUserId,
                request.UserRoundStatus,
                request.RoundStatus,
                request.IsPassed);

            if (totalRecords == 0)
                return new PaginationResult<IEnumerable<MyRoundsResultResponse>>([], 0, currentPage, pageSize);

            var myRounds = await _unitOfWork.UserRounds.GetMyRounds(
                currentUserId,
                request.UserRoundStatus,
                request.RoundStatus,
                request.IsPassed,
                skip,
                pageSize);

            var responses = myRounds
                .Select(x => new MyRoundsResultResponse
                {
                    RoundInfo = new SimpleRoundResponse
                    {
                        RoundId = x.RoundId,
                        RoundNumber = x.RoundNumber,
                        StartTime = x.StartTime,
                        EndTime = x.EndTime,
                        TimeLimit = x.TimeLimit,
                        RoundStatus = x.RoundStatus
                    },
                    UserRoundResult = new SimpleUserRoundResponse
                    {
                        Status = x.Status,
                        StartedAt = x.StartedAt,
                        SubmittedAt = x.SubmittedAt,
                        IsPassed = x.IsPassed,
                        Rank = x.Rank
                    }
                })
                .ToList();

            return new PaginationResult<IEnumerable<MyRoundsResultResponse>>(responses, totalRecords, currentPage, pageSize);
        }

        public async Task<IEnumerable<UserRoundResponseDto>> GetAllRoundResults(Guid roundId)
        {
            var round = await _unitOfWork.Rounds.GetByCondition(r => r.RoundID == roundId);
            if (round == null)
                throw new KeyNotFoundException($"Round with ID [{roundId}] not found.");

            var userRounds = await _unitOfWork.UserRounds.GetManyByCondition(
                ur => ur.RoundID == roundId
            );

            return userRounds.Select(ur => new UserRoundResponseDto
            {
                UserRoundID = ur.UserRoundID,
                Status = ur.Status,
                Point = ur.Point ?? 0,
                ExecutionTime = ur.ExecutionTime ?? TimeSpan.Zero,
                StartedAt = ur.StartedAt,
                SubmittedAt = ur.SubmittedAt ?? ur.StartedAt,
                IsPassed = ur.IsPassed ?? false,
                Rank = ur.Rank,
                Round = new SimpleRoundResponse
                {
                    RoundId = round.RoundID,
                    RoundNumber = round.RoundNumber,
                    StartTime = round.StartTime,
                    EndTime = round.EndTime,
                    TimeLimit = round.TimeLimit,
                    RoundStatus = round.Status
                }
            });
        }

        /// <summary>
        /// Lấy danh sách kết quả tất cả thí sinh trong một vòng thi theo điều kiện lọc, sắp xếp và phân trang.
        /// </summary>
        public async Task<RoundResultsDto> GetRoundResults(Guid roundId, RoundResultAllParicipations request)
        {
            var round = await _unitOfWork.Rounds.GetByCondition(r => r.RoundID == roundId);
            if (round == null)
                throw new KeyNotFoundException($"Không tìm thấy vòng thi với ID [{roundId}].");

            if (round.Status != RoundStatus.Valid || _clock.Now < round.EndTime)
                throw new InvalidOperationException("Vòng thi chưa kết thúc, chưa thể xem kết quả tổng hợp.");

            int currentPage = Math.Max(1, request.CurrentPage);
            int pageSize = Math.Max(1, request.PageSize);
            int skip = (currentPage - 1) * pageSize;

            int totalRecords = await _unitOfWork.UserRounds.CountRoundResults(roundId, request.Status);

            if (totalRecords == 0)
            {
                return new RoundResultsDto
                {
                    RoundInfo = BuildSimpleRound(round),
                    UserResults = new PaginationResult<IEnumerable<ParticipantResultResponse>>([], 0, currentPage, pageSize)
                };
            }

            var pageResults = (await _unitOfWork.UserRounds.GetRoundResults(
                roundId,
                request.Status,
                request.SortBy,
                request.SortDirection,
                skip,
                pageSize)).ToList();

            var userIds = pageResults
                .Select(x => x.UserId)
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            Dictionary<Guid, UserResponse> usersById = [];
            if (userIds.Count > 0)
            {
                try
                {
                    usersById = (await _identityMicroserviceClient.GetUsersBulk(userIds))
                        .GroupBy(x => x.UserId)
                        .ToDictionary(x => x.Key, x => x.First());
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Không thể lấy thông tin người dùng từ hệ thống định danh.", ex);
                }
            }

            var participants = pageResults
                .Select(x =>
                {
                    usersById.TryGetValue(x.UserId, out var user);
                    var fullName = AppHelper.GetFullName(user) ?? user?.Username ?? "Không xác định";

                    return new ParticipantResultResponse
                    {
                        UserInfo = new SimpleUserReponse
                        {
                            UserId = x.UserId,
                            FullName = fullName,
                            Email = user?.Email ?? string.Empty
                        },
                        ParticipantResult = new SimpleUserRoundResponse
                        {
                            Status = x.Status,
                            StartedAt = x.StartedAt,
                            SubmittedAt = x.SubmittedAt,
                            IsPassed = x.IsPassed,
                            Rank = x.Rank
                        }
                    };
                })
                .ToList();

            return new RoundResultsDto
            {
                RoundInfo = BuildSimpleRound(round),
                UserResults = new PaginationResult<IEnumerable<ParticipantResultResponse>>(
                    participants,
                    totalRecords,
                    currentPage,
                    pageSize)
            };
        }

        private static UserRoundResponseDto MapToUserRoundResponse(UserRound userRound, Round round)
        {
            return new UserRoundResponseDto
            {
                UserRoundID = userRound.UserRoundID,
                Status = userRound.Status,
                Point = userRound.Point ?? 0,
                ExecutionTime = userRound.ExecutionTime ?? TimeSpan.Zero,
                StartedAt = userRound.StartedAt,
                SubmittedAt = userRound.SubmittedAt ?? userRound.StartedAt,
                IsPassed = userRound.IsPassed ?? false,
                Rank = userRound.Rank,
                Round = new SimpleRoundResponse
                {
                    RoundId = round.RoundID,
                    RoundNumber = round.RoundNumber,
                    StartTime = round.StartTime,
                    EndTime = round.EndTime,
                    TimeLimit = round.TimeLimit,
                    RoundStatus = round.Status
                }
            };
        }

        private static SimpleRoundResponse BuildSimpleRound(Round round)
        {
            return new SimpleRoundResponse
            {
                RoundId = round.RoundID,
                RoundNumber = round.RoundNumber,
                StartTime = round.StartTime,
                EndTime = round.EndTime,
                TimeLimit = round.TimeLimit,
                RoundStatus = round.Status
            };
        }
    }
}
