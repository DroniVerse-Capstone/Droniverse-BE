using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;

public class CompetitionPrize
{
    public Guid CompetitionPrizeID { get; private set; }
    public Guid CompetitionID { get; private set; }
    public Competition Competition { get; private set; }
    public string TitleVN { get; private set; }
    public string TitleEN { get; private set; }
    public string? DescriptionVN { get; private set; }
    public string? DescriptionEN { get; private set; }
    public RewardType RewardType { get; private set; }
    public string? RewardValueGiftVN { get; private set; }
    public string? RewardValueGiftEN { get; private set; }
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
        DateTime createdAt,
        decimal? rewardValueMoney = null,
        string? rewardValueGiftVN = null,
        string? rewardValueGiftEN = null,
        string? descriptionVN = null,
        string? descriptionEN = null)
    {
        ValidateRank(rankFrom, rankTo);
        
        // Normalize values based on RewardType before validation
        if (rewardType == RewardType.MONEY)
        {
            rewardValueGiftVN = null;
            rewardValueGiftEN = null;
        }
        else if (rewardType == RewardType.GIFT)
        {
            rewardValueMoney = null;
        }

        ValidateReward(rewardType, rewardValueMoney, rewardValueGiftVN);

        CompetitionPrizeID = Guid.NewGuid();
        CompetitionID = competitionID;

        TitleVN = titleVN;
        TitleEN = titleEN;

        DescriptionVN = descriptionVN;
        DescriptionEN = descriptionEN;

        RewardType = rewardType;

        RewardValueMoney = rewardValueMoney;
        RewardValueGiftVN = rewardValueGiftVN;
        RewardValueGiftEN = rewardValueGiftEN;

        RankFrom = rankFrom;
        RankTo = rankTo;

        CreatedAt = createdAt;
        CreatedBy = createdBy;

        UserPrizes = new List<UserPrize>();
    }

    public void UpdateInformation(
        string titleVN,
        string titleEN,
         int rankFrom,
        int rankTo,
        Guid updatedBy,
        DateTime updateAt,
        string? descriptionVN = null,
        string? descriptionEN = null
       )
    {
        ValidateRank(rankFrom, rankTo);

        TitleVN = titleVN;
        TitleEN = titleEN;

        DescriptionVN = descriptionVN;
        DescriptionEN = descriptionEN;

        RankFrom = rankFrom;
        RankTo = rankTo;

        SetUpdated(updatedBy, updateAt);
    }

    public void UpdateFullInformation(
        string titleVN,
        string titleEN,
        RewardType rewardType,
        int rankFrom,
        int rankTo,
        Guid updatedBy,
        DateTime updatedAt,
        string? descriptionVN = null,
        string? descriptionEN = null,
        decimal? rewardValueMoney = null,
        string? rewardValueGiftVN = null,
        string? rewardValueGiftEN = null)
    {
        ValidateRank(rankFrom, rankTo);
        
        // Normalize values based on RewardType before validation
        if (rewardType == RewardType.MONEY)
        {
            rewardValueGiftVN = null;
            rewardValueGiftEN = null;
        }
        else if (rewardType == RewardType.GIFT)
        {
            rewardValueMoney = null;
        }

        ValidateReward(rewardType, rewardValueMoney, rewardValueGiftVN);

        TitleVN = titleVN;
        TitleEN = titleEN;

        DescriptionVN = descriptionVN;
        DescriptionEN = descriptionEN;

        RewardType = rewardType;
        RewardValueMoney = rewardValueMoney;
        RewardValueGiftVN = rewardValueGiftVN;
        RewardValueGiftEN = rewardValueGiftEN;

        RankFrom = rankFrom;
        RankTo = rankTo;

        SetUpdated(updatedBy, updatedAt);
    }

    public void UpdateMoneyReward(decimal amount, Guid updatedBy, DateTime updatedAt)
    {
        if (RewardType != RewardType.MONEY)
            throw new InvalidOperationException("Phần thưởng này không phải loại tiền.");

        if (amount <= 0)
            throw new ArgumentException("Số tiền thưởng phải lớn hơn 0.");

        RewardValueMoney = amount;

        SetUpdated(updatedBy, updatedAt);
    }

    public void UpdateGiftReward(string giftVN, string giftEN, Guid updatedBy, DateTime updateAt)
    {
        if (RewardType != RewardType.GIFT)
            throw new InvalidOperationException("Phần thưởng này không phải loại quà.");

        RewardValueGiftVN = giftVN;
        RewardValueGiftEN = giftEN;

        SetUpdated(updatedBy, updateAt);
    }

    private static void ValidateRank(int from, int to)
    {
        if (from <= 0 || to <= 0)
            throw new ArgumentException("Thứ hạng phải lớn hơn 0.");

        if (from > to)
            throw new ArgumentException("Thứ hạng bắt đầu không được lớn hơn thứ hạng kết thúc.");
    }

    private static void ValidateReward(
        RewardType type,
        decimal? money,
        string? giftVN)
    {
        if (type == RewardType.MONEY)
        {
            if (money == null || money <= 0)
                throw new ArgumentException("Phần thưởng tiền phải có giá trị lớn hơn 0.");
        }
        else if (type == RewardType.GIFT)
        {
            if (string.IsNullOrWhiteSpace(giftVN))
                throw new ArgumentException("Phần thưởng quà phải có mô tả quà (tiếng Việt).");
        }
    }

    private void SetUpdated(Guid updatedBy, DateTime updatedAt)
    {
        UpdatedBy = updatedBy;
        UpdatedAt = updatedAt;
    }
}