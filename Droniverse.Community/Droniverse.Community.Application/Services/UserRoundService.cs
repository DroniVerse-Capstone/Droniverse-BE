using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Shared.Services;

namespace Droniverse.Community.Application.Services
{
    public class UserRoundService : IUserRoundService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IClock _clock;

        public UserRoundService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IClock clock)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _clock = clock;
        }

        public async Task<UserRoundResponseDto> SubmitSolution(Guid roundId, UserRoundSubmitDto request)
        {
            var currentUserId = Guid.Parse(_currentUserService.UserID ?? throw new UnauthorizedAccessException("Người dùng chưa được xác thực."));

            var round = await _unitOfWork.Rounds.GetByCondition(r => r.RoundID == roundId);
            if (round == null)
                throw new KeyNotFoundException($"Round with ID {roundId} not found.");

            if (!round.IsActive(_clock.Now))
                throw new InvalidOperationException("Round is not active.");

            var userRound = await _unitOfWork.UserRounds.GetByCondition(
                ur => ur.UserID == currentUserId && ur.RoundID == roundId
            );

            if (userRound == null)
            {
                userRound = new UserRound(currentUserId, roundId, _clock.Now);
                await _unitOfWork.UserRounds.Add(userRound);
            }

            userRound.SubmitSolution(request.Solution, _clock.Now);

            await _unitOfWork.UserRounds.Update(userRound);
            await _unitOfWork.SaveChangeAsync();

            return MapToUserRoundResponse(userRound);
        }

        public async Task<UserRoundResponseDto> GetUserRoundResult(Guid roundId)
        {
            var currentUserId = Guid.Parse(_currentUserService.UserID ?? throw new UnauthorizedAccessException("Người dùng chưa được xác thực."));

            var userRound = await _unitOfWork.UserRounds.GetByCondition(
                ur => ur.UserID == currentUserId && ur.RoundID == roundId
            );

            if (userRound == null)
                throw new KeyNotFoundException("User has not participated in this round.");

            return MapToUserRoundResponse(userRound);
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
                UserID = ur.UserID,
                RoundID = ur.RoundID,
                Solution = ur.Solution,
                IsCompleted = ur.Status == UserRoundStatus.Completed,
                ExecutionTime = ur.ExecutionTime?.TotalSeconds ?? 0,
                NumberOfSteps = ur.NumberOfSteps ?? 0,
                PathLength = ur.PathLength ?? 0,
                FeedbackVN = ur.FeedbackVN,
                FeedbackEN = ur.FeedbackEN,
                Rating = ur.Rating ?? 0,
                Point = ur.Point ?? 0,
                SubmittedAt = ur.SubmittedAt
            });
        }

        private static UserRoundResponseDto MapToUserRoundResponse(UserRound userRound)
        {
            return new UserRoundResponseDto
            {
                UserRoundID = userRound.UserRoundID,
                UserID = userRound.UserID,
                RoundID = userRound.RoundID,
                Solution = userRound.Solution,
                IsCompleted = userRound.Status == UserRoundStatus.Completed,
                ExecutionTime = userRound.ExecutionTime?.TotalSeconds ?? 0,
                NumberOfSteps = userRound.NumberOfSteps ?? 0,
                PathLength = userRound.PathLength ?? 0,
                FeedbackVN = userRound.FeedbackVN,
                FeedbackEN = userRound.FeedbackEN,
                Rating = userRound.Rating ?? 0,
                Point = userRound.Point ?? 0,
                SubmittedAt = userRound.SubmittedAt
            };
        }
    }
}
