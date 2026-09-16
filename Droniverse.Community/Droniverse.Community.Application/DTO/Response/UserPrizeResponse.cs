using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Application.DTO.Response
{
    public class UserPrizeResponse
    {
        public required SimpleCompetitionResponse Competition { get; set; }
        public required SimpleCompetitionPrizeResponse Prize { get; set; }
    }

    public class SimpleCompetitionPrizeResponse
    {
        public Guid PrizeId { get; set; }
        public required string TitleVN { get; set; }
        public required string TitleEN { get; set; }
        public int Rank { get; set; }
        public RewardType RewardType { get; set; }
        public decimal? RewardValueMoney { get; set; }
        public string? RewardValueGiftVN { get; set; }
        public string? RewardValueGiftEN { get; set; }
        public DateTime AwardedAt { get; set; }
    }
}
