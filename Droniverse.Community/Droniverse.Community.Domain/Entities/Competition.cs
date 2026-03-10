using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Domain.Entities;

public class Competition
{
    public Guid CompetitionID { get; private set; }

    public Guid ClubID { get; private set; }
    public Club Club { get; private set; }

    public string NameVN { get; private set; }
    public string NameEN { get; private set; }

    public string? DescriptionVN { get; private set; }
    public string? DescriptionEN { get; private set; }

    public string RuleContent { get; private set; }

    public int? MaxParticipants { get; private set; }

    public DateTime RegistrationStartDate { get; private set; }
    public DateTime RegistrationEndDate { get; private set; }

    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }

    public CompetitionStatus Status { get; private set; }

    public DateTime? ResultPublishedAt { get; private set; }

    public Guid CreatedBy { get; private set; }
    public Guid? UpdatedBy { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public ICollection<CompetitionCertificate> CompetitionCertificates { get; private set; }
    public ICollection<UserCompetition> UserCompetitions { get; private set; }
    public ICollection<Round> Rounds { get; private set; }

    public ICollection<CompetitionPrize> CompetitionPrizes { get; private set; }
    public ICollection<UserPrize> UserPrizes { get; private set; }

    private Competition() { }

    public Competition(
        Guid clubID,
        string nameVN,
        string nameEN,
        string ruleContent,
        DateTime registrationStart,
        DateTime registrationEnd,
        DateTime startDate,
        DateTime endDate,
        Guid createdBy,
        int? maxParticipants = null,
        string descriptionVN = null,
        string descriptionEN = null)
    {
        ValidateRegistrationTime(registrationStart, registrationEnd);
        ValidateCompetitionTime(startDate, endDate);

        CompetitionID = Guid.NewGuid();

        ClubID = clubID;

        NameVN = nameVN;
        NameEN = nameEN;

        DescriptionVN = descriptionVN;
        DescriptionEN = descriptionEN;

        RuleContent = ruleContent;

        RegistrationStartDate = registrationStart;
        RegistrationEndDate = registrationEnd;

        StartDate = startDate;
        EndDate = endDate;

        MaxParticipants = maxParticipants;

        Status = CompetitionStatus.DRAFT;

        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;

        CompetitionCertificates = new List<CompetitionCertificate>();
        UserCompetitions = new List<UserCompetition>();
        Rounds = new List<Round>();
        CompetitionPrizes = new List<CompetitionPrize>();
        UserPrizes = new List<UserPrize>();
    }

    public void OpenRegistration(Guid updatedBy)
    {
        EnsureStatus(CompetitionStatus.DRAFT);

        Status = CompetitionStatus.OPEN;
        SetUpdated(updatedBy);
    }

    public void CloseRegistration(Guid updatedBy)
    {
        EnsureStatus(CompetitionStatus.OPEN);

        Status = CompetitionStatus.CLOSED;
        SetUpdated(updatedBy);
    }

    public void StartCompetition(Guid updatedBy)
    {
        EnsureStatus(CompetitionStatus.CLOSED);

        // Kiểm tra có ít nhất 1 round Pending
        if (!Rounds.Any(r => r.Status == RoundStatus.Pending))
            throw new InvalidOperationException("Không thể bắt đầu cuộc thi khi không có round nào ở trạng thái Pending. Các round có trạng thái SCHEDULE_INVALID phải được chỉnh sửa trước.");

        Status = CompetitionStatus.ONGOING;
        SetUpdated(updatedBy);
    }

    public void FinishCompetition(Guid updatedBy)
    {
        EnsureStatus(CompetitionStatus.ONGOING);

        Status = CompetitionStatus.FINISHED;
        SetUpdated(updatedBy);
    }

    public void CancelCompetition(Guid updatedBy)
    {
        if (Status == CompetitionStatus.FINISHED)
            throw new InvalidOperationException("Không thể hủy cuộc thi đã kết thúc.");

        Status = CompetitionStatus.CANCELLED;
        SetUpdated(updatedBy);
    }

    public CompetitionPrize AddPrize(
        string titleVN,
        string titleEN,
        RewardType rewardType,
        int rankFrom,
        int rankTo,
        Guid createdBy,
        decimal? rewardValueMoney = null,
        string? rewardValueGiftVN = null,
        string? rewardValueGiftEN = null,
        string? descriptionVN = null,
        string? descriptionEN = null)
    {
        bool overlap = CompetitionPrizes.Any(p =>
            rankFrom <= p.RankTo &&
            rankTo >= p.RankFrom);

        if (overlap)
            throw new InvalidOperationException("Khoảng thứ hạng của giải thưởng bị trùng.");

        var prize = new CompetitionPrize(
            CompetitionID,
            titleVN,
            titleEN,
            rewardType,
            rankFrom,
            rankTo,
            createdBy,
            rewardValueMoney,
            rewardValueGiftVN,
            rewardValueGiftEN,
            descriptionVN,
            descriptionEN
        );

        CompetitionPrizes.Add(prize);

        return prize;
    }

    public UserPrize AssignPrizeToUser(
        Guid userId,
        Guid prizeId,
        int rank,
        Guid createdBy)
    {
        if (Status != CompetitionStatus.FINISHED)
            throw new InvalidOperationException("Chỉ có thể trao thưởng sau khi cuộc thi kết thúc.");

        var prize = CompetitionPrizes.FirstOrDefault(x => x.CompetitionPrizeID == prizeId);

        if (prize == null)
            throw new InvalidOperationException("Không tìm thấy cấu hình giải thưởng.");

        if (rank < prize.RankFrom || rank > prize.RankTo)
            throw new InvalidOperationException("Thứ hạng không thuộc phạm vi giải thưởng.");

        bool rankUsed = UserPrizes.Any(x => x.Rank == rank);

        if (rankUsed)
            throw new InvalidOperationException("Thứ hạng này đã được trao.");

        var userPrize = new UserPrize(
            userId,
            CompetitionID,
            prize.CompetitionPrizeID,
            rank,
            prize.RewardType,
            prize.RewardValueMoney,
            prize.RewardValueGiftVN,
            prize.RewardValueGiftEN,
            createdBy
        );

        UserPrizes.Add(userPrize);

        return userPrize;
    }

    public void GeneratePrizesFromRanking(
        Dictionary<Guid, int> ranking,
        Guid createdBy)
    {
        if (Status != CompetitionStatus.FINISHED)
            throw new InvalidOperationException("Chỉ tạo giải thưởng sau khi cuộc thi kết thúc.");

        foreach (var item in ranking)
        {
            var userId = item.Key;
            var rank = item.Value;

            var prize = CompetitionPrizes.FirstOrDefault(p =>
                rank >= p.RankFrom && rank <= p.RankTo);

            if (prize == null)
                continue;

            bool alreadyAssigned = UserPrizes.Any(x => x.Rank == rank);

            if (alreadyAssigned)
                continue;

            var userPrize = new UserPrize(
                userId,
                CompetitionID,
                prize.CompetitionPrizeID,
                rank,
                prize.RewardType,
                prize.RewardValueMoney,
                prize.RewardValueGiftVN,
                prize.RewardValueGiftEN,
                createdBy
            );

            UserPrizes.Add(userPrize);
        }
    }

    public void PublishResult(Guid updatedBy)
    {
        if (Status != CompetitionStatus.FINISHED)
            throw new InvalidOperationException("Cuộc thi phải kết thúc trước khi công bố kết quả.");

        if (!UserPrizes.Any())
            throw new InvalidOperationException("Chưa có dữ liệu trao thưởng.");

        ResultPublishedAt = DateTime.UtcNow;

        SetUpdated(updatedBy);
    }

    public void UpdateInformation(
        string nameVN,
        string nameEN,
        string ruleContent,
        DateTime registrationStart,
        DateTime registrationEnd,
        DateTime startDate,
        DateTime endDate,
        Guid updatedBy,
        int? maxParticipants = null,
        string? descriptionVN = null,
        string? descriptionEN = null)
    {
        // Chỉ cho phép update khi status là DRAFT hoặc OPEN
        if (Status != CompetitionStatus.DRAFT && Status != CompetitionStatus.OPEN)
            throw new InvalidOperationException($"Không thể cập nhật cuộc thi khi đang ở trạng thái {Status}. Chỉ có thể cập nhật khi cuộc thi đang ở trạng thái DRAFT hoặc OPEN.");

        ValidateRegistrationTime(registrationStart, registrationEnd);
        ValidateCompetitionTime(startDate, endDate);

        NameVN = nameVN;
        NameEN = nameEN;
        DescriptionVN = descriptionVN;
        DescriptionEN = descriptionEN;
        RuleContent = ruleContent;
        MaxParticipants = maxParticipants;
        RegistrationStartDate = registrationStart;
        RegistrationEndDate = registrationEnd;
        StartDate = startDate;
        EndDate = endDate;

        SetUpdated(updatedBy);
    }

    public List<Guid> ValidateAndMarkInvalidRounds()
    {
        var invalidRoundIds = new List<Guid>();

        foreach (var round in Rounds.Where(r => r.Status == RoundStatus.Pending))
        {
            if (round.StartTime < StartDate || round.EndTime > EndDate)
            {
                round.MarkAsScheduleInvalid();
                invalidRoundIds.Add(round.RoundID);
            }
        }

        return invalidRoundIds;
    }

    public CompetitionCertificate AddCertificate(Guid certificateId)
    {
        if (Status != CompetitionStatus.DRAFT)
            throw new InvalidOperationException("Chỉ có thể thêm certificate khi cuộc thi đang ở trạng thái DRAFT.");

        var existingCertificate = CompetitionCertificates.FirstOrDefault(cc => cc.CertificateID == certificateId);
        if (existingCertificate != null)
            throw new InvalidOperationException("Certificate này đã được thêm vào cuộc thi rồi.");

        var competitionCertificate = new CompetitionCertificate
        {
            CompetitionID = CompetitionID,
            CertificateID = certificateId
        };

        CompetitionCertificates.Add(competitionCertificate);

        return competitionCertificate;
    }

    public void RemoveCertificate(Guid certificateId)
    {
        if (Status != CompetitionStatus.DRAFT)
            throw new InvalidOperationException("Chỉ có thể xóa certificate khi cuộc thi đang ở trạng thái DRAFT.");

        var certificate = CompetitionCertificates.FirstOrDefault(cc => cc.CertificateID == certificateId);
        if (certificate == null)
            throw new KeyNotFoundException("Không tìm thấy certificate trong cuộc thi này.");

        CompetitionCertificates.Remove(certificate);
    }

    private void EnsureStatus(CompetitionStatus requiredStatus)
    {
        if (Status != requiredStatus)
            throw new InvalidOperationException($"Trạng thái cuộc thi phải là {requiredStatus}.");
    }

    private void SetUpdated(Guid updatedBy)
    {
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void ValidateRegistrationTime(DateTime start, DateTime end)
    {
        if (start >= end)
            throw new ArgumentException("Thời gian bắt đầu đăng ký phải trước thời gian kết thúc đăng ký.");
    }

    private static void ValidateCompetitionTime(DateTime start, DateTime end)
    {
        if (start >= end)
            throw new ArgumentException("Thời gian bắt đầu cuộc thi phải trước thời gian kết thúc.");
    }
}