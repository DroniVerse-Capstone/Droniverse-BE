

using Droniverse.Community.Application.DTO.Extensions;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Shared.DTOs;

namespace Droniverse.Community.Application.Services
{
    public class UserPrizeService : IUserPrizeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IdentityMicroserviceClient _identityMicroserviceClient;
        private readonly ICurrentUserService _currentUserService;
        public UserPrizeService(IUnitOfWork unitOfWork, IdentityMicroserviceClient identityMicroserviceClient, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _identityMicroserviceClient = identityMicroserviceClient;
            _currentUserService = currentUserService;
        }

        public async Task<PaginationResult<IEnumerable<UserPrizeResponse>>> GetUserPrizeByCurrentUser(GetUserPrizeCurrentUserSearchRequest request)
        {
            var currentUserId = _currentUserService.UserId;

            var (items, total) = await _unitOfWork.UserPrizes.GetUserPrizeCurrentByUserId(currentUserId, request.CompetitionName, request.CurrentPage, request.PageSize);

            var result = items.Select(x => new UserPrizeResponse
            {
                Competition = new SimpleCompetitionResponse
                {
                    CompetitionID = x.CompetitionID,
                    NameVN = x.NameVN,
                    NameEN = x.NameEN
                },

                Prize = new SimpleCompetitionPrizeResponse
                {
                    PrizeId = x.PrizeId,
                    TitleVN = x.TitleVN,
                    TitleEN = x.TitleEN,
                    Rank = x.Rank,
                    RewardType = x.RewardType,
                    RewardValueMoney = x.RewardValueMoney,
                    RewardValueGiftVN = x.RewardValueGiftVN,
                    RewardValueGiftEN = x.RewardValueGiftEN,
                    AwardedAt = x.AwardedAt
                }
            });

            return new PaginationResult<IEnumerable<UserPrizeResponse>>(result, total, request.CurrentPage, request.PageSize);
        }
    }
}
