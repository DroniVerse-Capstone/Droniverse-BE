using Droniverse.Community.Application.DTO.Extensions;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.Helpers;
using System.ComponentModel.DataAnnotations;

public class UserRoundService : IUserRoundService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IdentityMicroserviceClient _identityMicroserviceClient;
    private readonly AcademyMicroserviceClient _academyMicroserviceClient;
    private readonly ICurrentUserService _currentUserService;
    private readonly IClock _clock;

    public UserRoundService(
        IUnitOfWork unitOfWork,
        IdentityMicroserviceClient identityMicroserviceClient,
        ICurrentUserService currentUserService,
        AcademyMicroserviceClient academyMicroserviceClient,
        IClock clock)
    {
        _unitOfWork = unitOfWork;
        _identityMicroserviceClient = identityMicroserviceClient;
        _currentUserService = currentUserService;
        _clock = clock;
        _academyMicroserviceClient = academyMicroserviceClient;
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

        if (userRound.Status == UserRoundStatus.Disqualified)
            throw new ValidationException("Người dùng đã bị cấm thi đấu.");

        userRound.Complete(
            request.ExecutionTime,
            request.Point,
            request.IsPassed,
            round.TimeLimit,
            now,
            round.EndTime
        );

        await _unitOfWork.UserRounds.Update(userRound);
        await _unitOfWork.SaveChangeAsync();

        return new SubmitSolutionResponse
        {
            UserRoundId = userRound.UserRoundID,
            SubmittedAt = userRound.SubmittedAt ?? userRound.GetEffectiveSubmittedAt(round.TimeLimit, now, round.EndTime),
            Status = userRound.Status
        };
    }

    public async Task<UserRoundResponseDto> GetUserRoundResult(Guid roundId)
    {
        var currentUserId = Guid.Parse(_currentUserService.UserID
            ?? throw new UnauthorizedAccessException("Người dùng chưa được xác thực."));

        var round = await _unitOfWork.Rounds.GetByCondition(r => r.RoundID == roundId);
        if (round == null)
            throw new KeyNotFoundException("Không tìm thấy vòng thi.");

        var userRound = await _unitOfWork.UserRounds.GetByCondition(
            ur => ur.UserID == currentUserId && ur.RoundID == roundId
        );

        if (userRound == null)
            throw new KeyNotFoundException("Bạn chưa tham gia vòng thi này.");

        var vr = await _academyMicroserviceClient.GetSimpleVRSimulator(round.VRSimilatorID);

        return MapToUserRoundResponse(userRound, round, vr);
    }

    public async Task<UserRoundResponseDto> GetRoundResultByUser(Guid userId, Guid roundId)
    {
        var round = await _unitOfWork.Rounds.GetByCondition(r => r.RoundID == roundId)
            ?? throw new KeyNotFoundException("Không tìm thấy vòng thi.");

        var result = await _unitOfWork.UserRounds.GetRoundResultByUser(userId, roundId)
            ?? throw new KeyNotFoundException("Người dùng chưa tham gia.");

        var vr = await _academyMicroserviceClient.GetSimpleVRSimulator(round.VRSimilatorID);

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
                VRSimulator = vr,
                RoundNumber = result.RoundNumber,
                StartTime = result.RoundStartTime,
                EndTime = result.RoundEndTime,
                RoundWeight = result.RoundWeight,
                TimeLimit = result.RoundTimeLimit,
                RoundStatus = result.RoundStatus
            }
        };
    }

    public async Task<PaginationResult<IEnumerable<MyRoundsResultResponse>>> GetMyUserRound(MyRoundSearchRequest request)
    {
        var userId = _currentUserService.UserId;

        int skip = (request.CurrentPage - 1) * request.PageSize;

        var total = await _unitOfWork.UserRounds.CountMyRounds(
            userId,
            request.UserRoundStatus,
            request.RoundStatus,
            request.IsPassed);

        if (total == 0)
            return new PaginationResult<IEnumerable<MyRoundsResultResponse>>([], 0, request.CurrentPage, request.PageSize);

        var data = await _unitOfWork.UserRounds.GetMyRounds(
            userId,
            request.UserRoundStatus,
            request.RoundStatus,
            request.IsPassed,
            skip,
            request.PageSize);

        var vrIds = data.Select(x => x.VRSimulatorId).Distinct().ToList();

        var vrDict = (await _academyMicroserviceClient.GetVRSimulatorsByIds(vrIds))
            .ToDictionary(x => x.VRSimulatorId, x => x);

        var result = data.Select(x => new MyRoundsResultResponse
        {
            RoundInfo = new SimpleRoundResponse
            {
                RoundId = x.RoundId,
                VRSimulator = vrDict.GetValueOrDefault(x.VRSimulatorId)!,
                RoundNumber = x.RoundNumber,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                RoundWeight = x.Weight,
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
        });

        return new PaginationResult<IEnumerable<MyRoundsResultResponse>>(result, total, request.CurrentPage, request.PageSize);
    }

    public async Task<IEnumerable<UserRoundResponseDto>> GetAllRoundResults(Guid roundId)
    {
        var round = await _unitOfWork.Rounds.GetByCondition(r => r.RoundID == roundId)
            ?? throw new KeyNotFoundException("Round not found");

        var vr = await _academyMicroserviceClient.GetSimpleVRSimulator(round.VRSimilatorID);

        var userRounds = await _unitOfWork.UserRounds.GetManyByCondition(x => x.RoundID == roundId);

        return userRounds.Select(ur => MapToUserRoundResponse(ur, round, vr));
    }

    public async Task<RoundResultsDto> GetRoundResults(Guid roundId, RoundResultAllParicipations request)
    {
        var round = await _unitOfWork.Rounds.GetByCondition(r => r.RoundID == roundId)
            ?? throw new KeyNotFoundException("Không tìm thấy round.");

        if (_clock.Now < round.EndTime)
            throw new InvalidOperationException("Chưa kết thúc round.");

        var vr = await _academyMicroserviceClient.GetSimpleVRSimulator(round.VRSimilatorID);

        int skip = (request.CurrentPage - 1) * request.PageSize;

        var page = (await _unitOfWork.UserRounds.GetRoundResults(
            roundId,
            request.Status,
            request.SortBy,
            request.SortDirection,
            skip,
            request.PageSize)).ToList();

        var userIds = page.Select(x => x.UserId).Distinct();

        var users = (await _identityMicroserviceClient.GetUsersBulk(userIds))
            .ToDictionary(x => x.UserId, x => x);

        var participants = page.Select(x =>
        {
            users.TryGetValue(x.UserId, out var user);

            return new ParticipantResultResponse
            {
                UserInfo = new SimpleUserReponse
                {
                    UserId = x.UserId,
                    FullName = AppHelper.GetFullName(user) ?? user?.Username,
                    Email = user?.Email ?? "",
                    AvatarUrl = user.ImageUrl,
                },
                ParticipantResult = new SimpleUserRoundResponse
                {
                    Status = x.Status,
                    StartedAt = x.StartedAt,
                    SubmittedAt = x.SubmittedAt,
                    ExecutionTime = x.ExecutionTime,
                    Point = x.Point,
                    IsPassed = x.IsPassed,
                    Rank = x.Rank
                }
            };
        });

        return new RoundResultsDto
        {
            RoundInfo = BuildSimpleRound(round, vr),
            UserResults = new PaginationResult<IEnumerable<ParticipantResultResponse>>(
                participants,
                page.Count,
                request.CurrentPage,
                request.PageSize)
        };
    }

    private static UserRoundResponseDto MapToUserRoundResponse(
        UserRound ur,
        Round round,
        SimpleVRSimulatorResponse vr)
    {
        return new UserRoundResponseDto
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
                VRSimulator = vr,
                RoundNumber = round.RoundNumber,
                StartTime = round.StartTime,
                EndTime = round.EndTime,
                RoundWeight = round.Weight,
                TimeLimit = round.TimeLimit,
                RoundStatus = round.Status
            }
        };
    }

    private static SimpleRoundResponse BuildSimpleRound(Round round, SimpleVRSimulatorResponse vr)
    {
        return new SimpleRoundResponse
        {
            RoundId = round.RoundID,
            VRSimulator = vr,
            RoundNumber = round.RoundNumber,
            StartTime = round.StartTime,
            EndTime = round.EndTime,
            RoundWeight = round.Weight,
            TimeLimit = round.TimeLimit,
            RoundStatus = round.Status
        };
    }
}
