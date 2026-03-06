using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Domain.Entities;

public class CompetitionPrize
{
    public Guid CompetitionPrizeID { get; private set; }

    public Guid CompetitionID { get; private set; }
    public Competition Competition { get; private set; }

    public string TitleVN { get; private set; }
    public string TitleEN { get; private set; }

    public string DescriptionVN { get; private set; }
    public string DescriptionEN { get; private set; }

    public RewardType RewardType { get; private set; }

    public string RewardValueGiftVN { get; private set; }
    public string RewardValueGiftEN { get; private set; }

    public decimal? RewardValueMoney { get; private set; }

    public int RankFrom { get; private set; }
    public int RankTo { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }

    public DateTime? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }
    public ICollection<UserPrize> UserPrizes { get; private set; }

    private CompetitionPrize() { }

    public CompetitionPrize(
        Guid competitionID,
        string titleVN,
        string titleEN,
        RewardType rewardType,
        int rankFrom,
        int rankTo,
        Guid createdBy,
        decimal? rewardValueMoney = null,
        string rewardValueGiftVN = null,
        string rewardValueGiftEN = null,
        string descriptionVN = null,
        string descriptionEN = null)
    {
        if (rankFrom <= 0 || rankTo <= 0)
            throw new ArgumentException("Rank must be greater than 0.");

        if (rankFrom > rankTo)
            throw new ArgumentException("RankFrom cannot be greater than RankTo.");

        CompetitionPrizeID = Guid.NewGuid();

        CompetitionID = competitionID;

        TitleVN = titleVN;
        TitleEN = titleEN;

        DescriptionVN = descriptionVN;
        DescriptionEN = descriptionEN;

        RewardType = rewardType;

        RewardValueGiftVN = rewardValueGiftVN;
        RewardValueGiftEN = rewardValueGiftEN;

        RewardValueMoney = rewardValueMoney;

        RankFrom = rankFrom;
        RankTo = rankTo;

        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;

        UserPrizes = new List<UserPrize>();
    }

    public void UpdateInformation(
        string titleVN,
        string titleEN,
        string descriptionVN,
        string descriptionEN,
        int rankFrom,
        int rankTo,
        Guid updatedBy)
    {
        if (rankFrom > rankTo)
            throw new ArgumentException("RankFrom cannot be greater than RankTo.");

        TitleVN = titleVN;
        TitleEN = titleEN;

        DescriptionVN = descriptionVN;
        DescriptionEN = descriptionEN;

        RankFrom = rankFrom;
        RankTo = rankTo;

        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    public void UpdateMoneyReward(decimal amount, Guid updatedBy)
    {
        if (RewardType != RewardType.MONEY)
            throw new InvalidOperationException("Prize type is not MONEY.");

        RewardValueMoney = amount;

        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    public void UpdateGiftReward(string giftVN, string giftEN, Guid updatedBy)
    {
        if (RewardType != RewardType.GIFT)
            throw new InvalidOperationException("Prize type is not GIFT.");

        RewardValueGiftVN = giftVN;
        RewardValueGiftEN = giftEN;

        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }
}