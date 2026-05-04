using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace Droniverse.Community.Application.Services
{
    public class CompetitionPrizeService : ICompetitionPrizeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IClock _clock;

        public CompetitionPrizeService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IClock clock)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _clock = clock;
        }

        public async Task<CompetitionPrizeResponseDto> CreatePrize(Guid competitionId, CompetitionPrizeCreateDto request)
        {
            var currentUserId = Guid.Parse(_currentUserService.UserID
                ?? throw new UnauthorizedAccessException("User is not authenticated."));

            var competition = await _unitOfWork.Competitions.GetByCondition(
                c => c.CompetitionID == competitionId,
                q => q.Include(c => c.CompetitionPrizes)
            );

            if (competition == null)
                throw new KeyNotFoundException($"Competition with ID [{competitionId}] not found.");

            var prize = competition.AddPrize(
                request.TitleVN,
                request.TitleEN,
                request.RewardType,
                request.RankFrom,
                request.RankTo,
                currentUserId,
                _clock.Now,
                request.RewardValueMoney,
                request.RewardValueGiftVN,
                request.RewardValueGiftEN,
                request.DescriptionVN,
                request.DescriptionEN
            );

            await _unitOfWork.CompetitionPrizes.Add(prize);

            await _unitOfWork.SaveChangeAsync();

            return new CompetitionPrizeResponseDto
            {
                CompetitionPrizeID = prize.CompetitionPrizeID,
                CompetitionID = prize.CompetitionID,
                TitleVN = prize.TitleVN,
                TitleEN = prize.TitleEN,
                DescriptionVN = prize.DescriptionVN,
                DescriptionEN = prize.DescriptionEN,
                RewardType = prize.RewardType,
                RewardValueMoney = prize.RewardValueMoney,
                RewardValueGiftVN = prize.RewardValueGiftVN,
                RewardValueGiftEN = prize.RewardValueGiftEN,
                RankFrom = prize.RankFrom,
                RankTo = prize.RankTo,
                CreatedAt = prize.CreatedAt,
                UpdatedAt = prize.UpdatedAt
            };
        }

        public async Task<CompetitionPrizeResponseDto> UpdatePrize(Guid competitionPrizeId, CompetitionPrizeUpdateDto request)
        {
            var currentUserId = Guid.Parse(_currentUserService.UserID
                ?? throw new UnauthorizedAccessException("User is not authenticated."));

            var prize = await _unitOfWork.CompetitionPrizes.GetByCondition(p => p.CompetitionPrizeID == competitionPrizeId);
            if (prize == null)
                throw new KeyNotFoundException($"Prize with ID {competitionPrizeId} not found.");

            prize.UpdateFullInformation(
                request.TitleVN,
                request.TitleEN,
                request.RewardType,
                request.RankFrom,
                request.RankTo,
                currentUserId,
                _clock.Now,
                request.DescriptionVN,
                request.DescriptionEN,
                request.RewardValueMoney,
                request.RewardValueGiftVN,
                request.RewardValueGiftEN
            );

            await _unitOfWork.CompetitionPrizes.Update(prize);
            await _unitOfWork.SaveChangeAsync();

            return new CompetitionPrizeResponseDto
            {
                CompetitionPrizeID = prize.CompetitionPrizeID,
                CompetitionID = prize.CompetitionID,
                TitleVN = prize.TitleVN,
                TitleEN = prize.TitleEN,
                DescriptionVN = prize.DescriptionVN,
                DescriptionEN = prize.DescriptionEN,
                RewardType = prize.RewardType,
                RewardValueMoney = prize.RewardValueMoney,
                RewardValueGiftVN = prize.RewardValueGiftVN,
                RewardValueGiftEN = prize.RewardValueGiftEN,
                RankFrom = prize.RankFrom,
                RankTo = prize.RankTo,
                CreatedAt = prize.CreatedAt,
                UpdatedAt = prize.UpdatedAt
            };
        }

        public async Task<DeletePrizeResponse> DeletePrize(Guid competitionPrizeId)
        {
            var prize = await _unitOfWork.CompetitionPrizes.GetByCondition(p => p.CompetitionPrizeID == competitionPrizeId);
            if (prize == null)
                throw new KeyNotFoundException($"Không tìm thấy giải thưởng với ID [{competitionPrizeId}].");

            var competitionId = prize.CompetitionID;

            await _unitOfWork.CompetitionPrizes.Delete(prize);
            await _unitOfWork.SaveChangeAsync();

            var remainingPrizes = await _unitOfWork.CompetitionPrizes.GetManyByCondition(
                p => p.CompetitionID == competitionId,
                q => q.OrderBy(p => p.RankFrom).AsNoTracking()
            );

            var remainingPrizeResponses = remainingPrizes.Select(p => new CompetitionPrizeResponseDto
            {
                CompetitionPrizeID = p.CompetitionPrizeID,
                CompetitionID = p.CompetitionID,
                TitleVN = p.TitleVN,
                TitleEN = p.TitleEN,
                DescriptionVN = p.DescriptionVN,
                DescriptionEN = p.DescriptionEN,
                RewardType = p.RewardType,
                RewardValueMoney = p.RewardValueMoney,
                RewardValueGiftVN = p.RewardValueGiftVN,
                RewardValueGiftEN = p.RewardValueGiftEN,
                RankFrom = p.RankFrom,
                RankTo = p.RankTo,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            });

            return new DeletePrizeResponse
            {
                TotalDeleted = 1,
                RemainingPrizes = remainingPrizeResponses
            };
        }

        public async Task<IEnumerable<CompetitionPrizeResponseDto>> GetPrizesByCompetition(Guid competitionId)
        {
            var competition = await _unitOfWork.Competitions.GetByCondition(c => c.CompetitionID == competitionId);
            if (competition == null)
                throw new KeyNotFoundException($"Competition with ID {competitionId} not found.");

            var prizes = await _unitOfWork.CompetitionPrizes.GetManyByCondition(
                p => p.CompetitionID == competitionId,
                q => q.OrderBy(p => p.RankFrom)
            );

            return prizes.Select(prize => new CompetitionPrizeResponseDto
            {
                CompetitionPrizeID = prize.CompetitionPrizeID,
                CompetitionID = prize.CompetitionID,
                TitleVN = prize.TitleVN,
                TitleEN = prize.TitleEN,
                DescriptionVN = prize.DescriptionVN,
                DescriptionEN = prize.DescriptionEN,
                RewardType = prize.RewardType,
                RewardValueMoney = prize.RewardValueMoney,
                RewardValueGiftVN = prize.RewardValueGiftVN,
                RewardValueGiftEN = prize.RewardValueGiftEN,
                RankFrom = prize.RankFrom,
                RankTo = prize.RankTo,
                CreatedAt = prize.CreatedAt,
                UpdatedAt = prize.UpdatedAt
            });
        }
    }
}
