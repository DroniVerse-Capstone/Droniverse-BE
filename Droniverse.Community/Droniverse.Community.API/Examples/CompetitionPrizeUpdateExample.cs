using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Domain.Enums;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Community.API.Examples
{
    public class CompetitionPrizeUpdateExample : IExamplesProvider<CompetitionPrizeUpdateDto>
    {
        public CompetitionPrizeUpdateDto GetExamples()
        {
            return new CompetitionPrizeUpdateDto
            {
                TitleVN = "Giải Nhất (Cập nhật)",
                TitleEN = "First Prize (Updated)",
                DescriptionVN = "Giải thưởng dành cho thí sinh đạt hạng 1 - Đã cập nhật",
                DescriptionEN = "Prize for the 1st place winner - Updated",
                RewardType = RewardType.MONEY,
                RewardValueMoney = 10000000,
                RankFrom = 1,
                RankTo = 1
            };
        }
    }
}
