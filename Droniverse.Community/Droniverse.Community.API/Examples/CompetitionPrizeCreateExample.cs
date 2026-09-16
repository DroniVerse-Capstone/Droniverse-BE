using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Domain.Enums;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Community.API.Examples
{
    public class CompetitionPrizeCreateExample : IMultipleExamplesProvider<CompetitionPrizeCreateDto>
    {
        public IEnumerable<SwaggerExample<CompetitionPrizeCreateDto>> GetExamples()
        {
            yield return SwaggerExample.Create(
                "First Prize - Money",
                new CompetitionPrizeCreateDto
                {
                    TitleVN = "Giải Nhất",
                    TitleEN = "First Prize",
                    DescriptionVN = "Giải thưởng dành cho thí sinh đạt hạng 1 với phần thưởng tiền mặt.",
                    DescriptionEN = "Prize for the 1st place winner with a cash reward.",
                    RewardType = RewardType.MONEY,
                    RewardValueMoney = 10000000,
                    RankFrom = 1,
                    RankTo = 1
                }
            );

            yield return SwaggerExample.Create(
                "Second Prize - Gift",
                new CompetitionPrizeCreateDto
                {
                    TitleVN = "Giải Nhì",
                    TitleEN = "Second Prize",
                    DescriptionVN = "Giải thưởng dành cho thí sinh đạt hạng 2 với phần quà là laptop.",
                    DescriptionEN = "Prize for the 2nd place winner with a laptop gift.",
                    RewardType = RewardType.GIFT,
                    RewardValueGiftVN = "Laptop Dell Inspiron 14",
                    RewardValueGiftEN = "Dell Inspiron 14 Laptop",
                    RankFrom = 2,
                    RankTo = 5
                }
            );
        }
    }
}
