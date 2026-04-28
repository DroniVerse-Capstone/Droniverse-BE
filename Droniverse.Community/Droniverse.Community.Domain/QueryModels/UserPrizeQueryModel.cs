using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Domain.QueryModels
{
    public record UserPrizeQueryModel
    {
        public Guid CompetitionID { get; set; }
        public required string NameVN { get; set; }
        public required string NameEN { get; set; }
        public Guid PrizeId { get; set; }
        public required string TitleVN { get; set; }
        public required string TitleEN { get; set; }
        public int Rank { get; set; }
        public RewardType RewardType { get; set; }
        public decimal? RewardValueMoney { get; set; }
        public string? RewardValueGiftVN { get; set; }
        public string? RewardValueGiftEN { get; set; }
        public DateTime AwardedAt { get;  set; }
    }
}
