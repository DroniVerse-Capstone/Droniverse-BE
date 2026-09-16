using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Application.DTO.Response
{
    public class CompetitionPrizeResponseDto
    {
        public Guid CompetitionPrizeID { get; set; }
        public Guid CompetitionID { get; set; }
        public string TitleVN { get; set; }
        public string TitleEN { get; set; }
        public string? DescriptionVN { get; set; }
        public string? DescriptionEN { get; set; }
        public RewardType RewardType { get; set; }
        public decimal? RewardValueMoney { get; set; }
        public string? RewardValueGiftVN { get; set; }
        public string? RewardValueGiftEN { get; set; }
        public int RankFrom { get; set; }
        public int RankTo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
