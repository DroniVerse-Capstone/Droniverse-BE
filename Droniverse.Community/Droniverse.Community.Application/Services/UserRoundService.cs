using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace Droniverse.Community.Application.Services
{
    public class UserRoundService : IUserRoundService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public UserRoundService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<UserRoundResponseDto> SubmitSolution(Guid roundId, UserRoundSubmitDto request)
        {
            var currentUserId = Guid.Parse(_currentUserService.UserID ?? throw new UnauthorizedAccessException("Người dùng chưa được xác thực."));

            var round = await _unitOfWork.Rounds.GetByCondition(r => r.RoundID == roundId);
            if (round == null)
                throw new KeyNotFoundException($"Round with ID {roundId} not found.");

            if (!round.IsActive())
                throw new InvalidOperationException("Round is not active.");

            var userRound = await _unitOfWork.UserRounds.GetByCondition(
                ur => ur.UserID == currentUserId && ur.RoundID == roundId
            );

            if (userRound == null)
            {
                userRound = new UserRound(currentUserId, roundId);
                await _unitOfWork.UserRounds.Add(userRound);
            }

            userRound.SubmitSolution(request.Solution);

            await _unitOfWork.UserRounds.Update(userRound);
            await _unitOfWork.SaveChangeAsync();

            return new UserRoundResponseDto
            {
                UserRoundID = userRound.UserRoundID,
                UserID = userRound.UserID,
                RoundID = userRound.RoundID,
                Solution = userRound.Solution,
                IsCompleted = userRound.IsCompleted,
                ExecutionTime = userRound.ExecutionTime,
                NumberOfSteps = userRound.NumberOfSteps,
                PathLength = userRound.PathLength,
                FeedbackVN = userRound.FeedbackVN,
                FeedbackEN = userRound.FeedbackEN,
                Rating = userRound.Rating,
                Point = userRound.Point,
                SubmittedAt = userRound.SubmittedAt
            };
        }

        public async Task<UserRoundResponseDto> GetUserRoundResult(Guid roundId)
        {
            var currentUserId = Guid.Parse(_currentUserService.UserID ?? throw new UnauthorizedAccessException("Người dùng chưa được xác thực."));

            var userRound = await _unitOfWork.UserRounds.GetByCondition(
                ur => ur.UserID == currentUserId && ur.RoundID == roundId
            );

            if (userRound == null)
                throw new KeyNotFoundException("User has not participated in this round.");

            return new UserRoundResponseDto
            {
                UserRoundID = userRound.UserRoundID,
                UserID = userRound.UserID,
                RoundID = userRound.RoundID,
                Solution = userRound.Solution,
                IsCompleted = userRound.IsCompleted,
                ExecutionTime = userRound.ExecutionTime,
                NumberOfSteps = userRound.NumberOfSteps,
                PathLength = userRound.PathLength,
                FeedbackVN = userRound.FeedbackVN,
                FeedbackEN = userRound.FeedbackEN,
                Rating = userRound.Rating,
                Point = userRound.Point,
                SubmittedAt = userRound.SubmittedAt
            };
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
                IsCompleted = ur.IsCompleted,
                ExecutionTime = ur.ExecutionTime,
                NumberOfSteps = ur.NumberOfSteps,
                PathLength = ur.PathLength,
                FeedbackVN = ur.FeedbackVN,
                FeedbackEN = ur.FeedbackEN,
                Rating = ur.Rating,
                Point = ur.Point,
                SubmittedAt = ur.SubmittedAt
            });
        }
    }
}
