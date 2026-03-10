using Droniverse.Community.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Droniverse.Community.Application.DTO.Request
{
    public class CompetitionPrizeCreateDto
    {
        [Required]
        public Guid CompetitionID { get; set; }

        [Required]
        [StringLength(255)]
        public string TitleVN { get; set; }

        [Required]
        [StringLength(255)]
        public string TitleEN { get; set; }

        public string? DescriptionVN { get; set; }

        public string? DescriptionEN { get; set; }

        [Required]
        public RewardType RewardType { get; set; }

        public decimal? RewardValueMoney { get; set; }

        public string? RewardValueGiftVN { get; set; }

        public string? RewardValueGiftEN { get; set; }

        [Required]
        [Range(1, 10000)]
        public int RankFrom { get; set; }

        [Required]
        [Range(1, 10000)]
        public int RankTo { get; set; }
    }
}
