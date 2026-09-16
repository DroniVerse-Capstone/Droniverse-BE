using Droniverse.Community.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Domain.Entities
{
    public class UserPrize
    {
        public Guid UserPrizeID { get; private set; }

        public Guid UserID { get; private set; }
        public Guid CompetitionID { get; private set; }
        public Guid PrizeID { get; private set; }

        public int Rank { get; private set; }

        public RewardType RewardType { get; private set; }

        public decimal? RewardValueMoney { get; private set; }

        public string? RewardValueGiftVN { get; private set; }
        public string? RewardValueGiftEN { get; private set; }
        public bool IsAwarded { get; private set; }
        public DateTime? AwardedAt { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public Guid CreatedBy { get; private set; }

        public DateTime? UpdatedAt { get; private set; }
        public Guid? UpdatedBy { get; private set; }

        public Competition Competition { get; private set; } = null!;
        public CompetitionPrize Prize { get; private set; } = null!;

        private UserPrize() { }
        public UserPrize(
            Guid userId,
            Guid competitionId,
            Guid prizeId,
            int rank,
            RewardType rewardType,
            decimal? rewardValueMoney,
            string? rewardValueGiftVN,
            string? rewardValueGiftEN,
            Guid createdBy,
            DateTime createdAt)
        {
            UserPrizeID = Guid.NewGuid();
            UserID = userId;
            CompetitionID = competitionId;
            PrizeID = prizeId;

            Rank = rank;
            RewardType = rewardType;

            RewardValueMoney = rewardValueMoney;
            RewardValueGiftVN = rewardValueGiftVN;
            RewardValueGiftEN = rewardValueGiftEN;

            IsAwarded = false;

            CreatedAt = createdAt;
            CreatedBy = createdBy;
        }


        public void Award(Guid awardedBy, DateTime awardedAt, DateTime updatedAt)
        {
            if (IsAwarded)
                throw new InvalidOperationException("Prize already awarded");

            IsAwarded = true;
            AwardedAt = awardedAt;

            UpdatedAt = updatedAt;
            UpdatedBy = awardedBy;
        }


        public void UpdateGiftInfo(string? giftVN, string? giftEN, Guid updatedBy, DateTime updatedAt)
        {
            RewardValueGiftVN = giftVN;
            RewardValueGiftEN = giftEN;

            UpdatedAt = updatedAt;
            UpdatedBy = updatedBy;
        }
    }

    public static class UserPrizeFactory
    {
        public static UserPrize Create(
            Guid userId,
            Guid competitionId,
            Guid prizeId,
            int rank,
            RewardType rewardType,
            decimal? rewardValueMoney,
            string? rewardValueGiftVN,
            string? rewardValueGiftEN,
            Guid createdBy,
            DateTime createdAt)
        {
            return new UserPrize(
                userId,
                competitionId,
                prizeId,
                rank,
                rewardType,
                rewardValueMoney,
                rewardValueGiftVN,
                rewardValueGiftEN,
                createdBy,
                createdAt
            );
        }
    }
}
