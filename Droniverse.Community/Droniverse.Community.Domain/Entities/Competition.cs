using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.Services;
using System.Security.Policy;

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
    public DateTime VisibleAt { get; private set; }

    public DateTime RegistrationStartDate { get; private set; }
    public DateTime RegistrationEndDate { get; private set; }

    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }

    public CompetitionStatus Status { get; private set; }
    public DateTime? ResultPublishedAt { get; private set; }
    public CompetitionInvalidReason? InvalidReason { get; private set; }
    public DateTime? InvalidAt { get; private set; }
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
        DateTime visibleAt,
        DateTime registrationStart,
        DateTime registrationEnd,
        DateTime startDate,
        DateTime endDate,
        Guid createdBy,
        DateTime now,
        int? maxParticipants = null,
        string? descriptionVN = null,
        string? descriptionEN = null)
    {
        ValidateRegistrationTime(registrationStart, registrationEnd);
        ValidateCompetitionTime(startDate, endDate);
        ValidateTimeline(visibleAt, registrationStart, registrationEnd, startDate, endDate);
        CompetitionID = Guid.NewGuid();

        ClubID = clubID;

        NameVN = nameVN;
        NameEN = nameEN;

        DescriptionVN = descriptionVN;
        DescriptionEN = descriptionEN;

        RuleContent = ruleContent;

        VisibleAt = visibleAt;

        RegistrationStartDate = registrationStart;
        RegistrationEndDate = registrationEnd;

        StartDate = startDate;
        EndDate = endDate;

        MaxParticipants = maxParticipants;

        Status = CompetitionStatus.DRAFT;

        CreatedBy = createdBy;
        CreatedAt = now;

        CompetitionCertificates = [];
        UserCompetitions = [];
        Rounds = [];
        CompetitionPrizes = [];
        UserPrizes = [];
    }

    public void OpenRegistration(Guid updatedBy, DateTime now)
    {
        EnsureStatus(CompetitionStatus.DRAFT);
        Status = CompetitionStatus.REGISTRATION_OPEN;
        SetUpdated(updatedBy, now);
    }

    public void CloseRegistration(Guid updatedBy, DateTime now)
    {
        EnsureStatus(CompetitionStatus.REGISTRATION_OPEN);

        Status = CompetitionStatus.REGISTRATION_CLOSED;
        SetUpdated(updatedBy, now);
    }

    public void StartCompetition(Guid updatedBy, DateTime now)
    {
        EnsureStatus(CompetitionStatus.REGISTRATION_CLOSED);

        // Kiểm tra có ít nhất 1 round Pending
        if (!Rounds.Any(r => r.Status == RoundStatus.Pending))
            throw new InvalidOperationException("Không thể bắt đầu cuộc thi khi không có round nào ở trạng thái Pending. Các round có trạng thái [SCHEDULE_INVALID] phải được chỉnh sửa trước.");

        if (UserCompetitions.Count == 0)
            throw new InvalidOperationException("Không thể bắt đầu cuộc thi khi chưa có người tham gia.");

        Status = CompetitionStatus.ONGOING;
        SetUpdated(updatedBy, now);
    }

    public void FinishCompetition(Guid updatedBy, DateTime now)
    {
        EnsureStatus(CompetitionStatus.ONGOING);

        Status = CompetitionStatus.FINISHED;
        SetUpdated(updatedBy, now);
    }

    public void CancelCompetition(Guid updatedBy, DateTime now)
    {
        if (Status == CompetitionStatus.FINISHED)
            throw new InvalidOperationException("Không thể hủy cuộc thi đã kết thúc.");

        Status = CompetitionStatus.CANCELLED;
        SetUpdated(updatedBy, now);
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

    public void PublishResult(Guid updatedBy, DateTime now)
    {
        if (Status != CompetitionStatus.FINISHED)
            throw new InvalidOperationException("Cuộc thi phải kết thúc trước khi công bố kết quả.");

        if (!UserPrizes.Any())
            throw new InvalidOperationException("Chưa có dữ liệu trao thưởng.");

        ResultPublishedAt = now;

        Status = CompetitionStatus.RESULT_PUBLISHED;
        SetUpdated(updatedBy, now);
    }

    public void UpdateDraftInformation(
    string nameVN,
    string nameEN,
    string ruleContent,
    DateTime visibleAt,
    DateTime registrationStart,
    DateTime registrationEnd,
    DateTime startDate,
    DateTime endDate,
    Guid updatedBy,
    DateTime now,
    int? maxParticipants = null,
    string? descriptionVN = null,
    string? descriptionEN = null)
    {
        EnsureStatus(CompetitionStatus.DRAFT);

        ValidateRegistrationTime(registrationStart, registrationEnd);
        ValidateCompetitionTime(startDate, endDate);
        ValidateTimeline(visibleAt, registrationStart, registrationEnd, startDate, endDate);

        NameVN = nameVN;
        NameEN = nameEN;
        DescriptionVN = descriptionVN;
        DescriptionEN = descriptionEN;
        RuleContent = ruleContent;

        VisibleAt = visibleAt;
        RegistrationStartDate = registrationStart;
        RegistrationEndDate = registrationEnd;
        StartDate = startDate;
        EndDate = endDate;

        MaxParticipants = maxParticipants;

        SetUpdated(updatedBy, now);
    }

    public void UpdatePublishedInformation(
      string nameVN,
      string nameEN,
      string? descriptionVN,
      string? descriptionEN,
      int? maxParticipants,
      string? ruleContent,
      Guid updatedBy,
      DateTime now)
    {
        EnsureStatus(CompetitionStatus.PUBLISHED);

        NameVN = nameVN;
        NameEN = nameEN;
        DescriptionVN = descriptionVN;
        DescriptionEN = descriptionEN;

        if (maxParticipants.HasValue && maxParticipants != MaxParticipants)
        {
            if (maxParticipants.Value < UserCompetitions.Count)
                throw new InvalidOperationException("Không thể giảm số lượng nhỏ hơn số đã đăng ký.");

            MaxParticipants = maxParticipants;
        }

        if (!string.IsNullOrWhiteSpace(ruleContent) && ruleContent != RuleContent)
        {
            if (UserCompetitions.Count != 0)
                throw new InvalidOperationException("Không thể thay đổi luật khi đã có người đăng ký.");

            RuleContent = ruleContent;
        }

        SetUpdated(updatedBy, now);
    }

    public UserCompetition RegisterParticipant(Guid userId, DateTime now)
    {
        if (Status != CompetitionStatus.REGISTRATION_OPEN)
            throw new InvalidOperationException("Cuộc thi không trong trạng thái cho phép đăng ký.");

        if (now < RegistrationStartDate || now > RegistrationEndDate)
            throw new InvalidOperationException("Ngoài thời gian đăng ký.");

        if (MaxParticipants.HasValue && UserCompetitions.Count >= MaxParticipants.Value)
            throw new InvalidOperationException("Cuộc thi đã đủ số lượng người tham gia.");

        bool alreadyJoined = UserCompetitions.Any(x => x.UserID == userId);
        if (alreadyJoined)
            throw new InvalidOperationException("Người dùng đã đăng ký.");

        var userCompetition = new UserCompetition(userId, CompetitionID);

        UserCompetitions.Add(userCompetition);

        return userCompetition;
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

        var certificate = CompetitionCertificates.FirstOrDefault(cc => cc.CertificateID == certificateId) ?? throw new KeyNotFoundException("Không tìm thấy certificate trong cuộc thi này.");
        CompetitionCertificates.Remove(certificate);
    }

    public void UpdateStatus(
        CompetitionStatus targetStatus,
        Guid updatedBy,
        DateTime now,
        CompetitionInvalidReason? invalidReason = null)
    {
        if (Status == targetStatus)
            throw new InvalidOperationException($"Cuộc thi đã ở trạng thái [{targetStatus}].");

        switch (targetStatus)
        {
            case CompetitionStatus.PUBLISHED:
                EnsureStatus(CompetitionStatus.DRAFT);
                Status = CompetitionStatus.PUBLISHED;
                break;

            case CompetitionStatus.REGISTRATION_OPEN:
                EnsureStatus(CompetitionStatus.PUBLISHED);
                if (now < RegistrationStartDate)
                    throw new InvalidOperationException("Chưa đến thời gian mở đăng ký.");
                Status = CompetitionStatus.REGISTRATION_OPEN;
                break;

            case CompetitionStatus.REGISTRATION_CLOSED:
                EnsureStatus(CompetitionStatus.REGISTRATION_OPEN);
                if (now < RegistrationEndDate)
                    throw new InvalidOperationException("Chưa đến thời gian đóng đăng ký.");
                if (now >= StartDate)
                    throw new InvalidOperationException("Không thể đóng đăng ký khi đã qua thời điểm bắt đầu cuộc thi.");
                Status = CompetitionStatus.REGISTRATION_CLOSED;
                break;

            case CompetitionStatus.ONGOING:
                EnsureStatus(CompetitionStatus.REGISTRATION_CLOSED);
                if (now < StartDate)
                    throw new InvalidOperationException("Chưa đến thời gian bắt đầu cuộc thi.");
                if (!Rounds.Any(r => r.Status == RoundStatus.Pending))
                    throw new InvalidOperationException("Không thể bắt đầu cuộc thi khi không có round nào ở trạng thái Pending. Các round có trạng thái [SCHEDULE_INVALID] phải được chỉnh sửa trước.");
                if (UserCompetitions.Count == 0)
                    throw new InvalidOperationException("Không thể bắt đầu cuộc thi khi chưa có người tham gia.");
                Status = CompetitionStatus.ONGOING;
                break;

            case CompetitionStatus.FINISHED:
                EnsureStatus(CompetitionStatus.ONGOING);
                if (now < EndDate)
                    throw new InvalidOperationException("Chưa đến thời gian kết thúc cuộc thi.");
                Status = CompetitionStatus.FINISHED;
                break;

            case CompetitionStatus.RESULT_PUBLISHED:
                EnsureStatus(CompetitionStatus.FINISHED);
                if (!UserPrizes.Any())
                    throw new InvalidOperationException("Chưa có dữ liệu trao thưởng.");
                ResultPublishedAt = now;
                Status = CompetitionStatus.RESULT_PUBLISHED;
                break;

            case CompetitionStatus.CANCELLED:
                if (Status == CompetitionStatus.FINISHED || Status == CompetitionStatus.RESULT_PUBLISHED)
                    throw new InvalidOperationException("Không thể hủy cuộc thi đã kết thúc hoặc đã công bố kết quả.");
                Status = CompetitionStatus.CANCELLED;
                break;

            case CompetitionStatus.INVALID:
                if (!invalidReason.HasValue)
                    throw new InvalidOperationException("Cần cung cấp lý do khi chuyển sang trạng thái INVALID.");
                if (Status == CompetitionStatus.FINISHED || Status == CompetitionStatus.RESULT_PUBLISHED)
                    throw new InvalidOperationException($"Không thể [{CompetitionStatus.INVALID}] khi đã kết thúc.");
                InvalidReason = invalidReason.Value;
                InvalidAt = now;
                Status = CompetitionStatus.INVALID;
                break;

            default:
                throw new InvalidOperationException($"Không hỗ trợ chuyển sang trạng thái [{targetStatus}].");
        }

        SetUpdated(updatedBy, now);
    }

    // ======== CÁC HÀM VALIDATE CHO BACKGROUND JOB ========
    public bool CanAutoOpenRegistration(DateTime now)
    {
        return Status == CompetitionStatus.PUBLISHED && now >= RegistrationStartDate;
    }

    public bool CanAutoCloseRegistration(DateTime now)
    {
        return Status == CompetitionStatus.REGISTRATION_OPEN && now >= RegistrationEndDate && now < StartDate;
    }

    public bool CanAutoStartCompetition(DateTime now)
    {
        return (Status == CompetitionStatus.REGISTRATION_OPEN || Status == CompetitionStatus.REGISTRATION_CLOSED) && now >= StartDate;
    }

    public bool CanAutoFinishCompetition(DateTime now)
    {
        return Status == CompetitionStatus.ONGOING && now >= EndDate;
    }

    public bool CanAutoInvalidCompetition(DateTime now)
    {
        return Status == CompetitionStatus.REGISTRATION_OPEN
            && now >= StartDate
            && !Rounds.Any(r => r.Status == RoundStatus.Pending);
    }

   public bool CanAutoPublish(DateTime now)
{
    return Status == CompetitionStatus.DRAFT
        && now >= VisibleAt;
}

    // Các hàm thực thi (dùng cho hệ thống hoặc cron job gọi)
    public void SystemOpenRegistration(DateTime now)
    {
        EnsureStatus(CompetitionStatus.PUBLISHED);
        Status = CompetitionStatus.REGISTRATION_OPEN;
        UpdatedAt = now;
    }
    
    public void SystemCloseRegistration(DateTime now)
    {
        EnsureStatus(CompetitionStatus.REGISTRATION_OPEN);
        Status = CompetitionStatus.REGISTRATION_CLOSED;
        UpdatedAt = now; 
    }

    public void SystemStartCompetition(DateTime now)
    {
        if (Status != CompetitionStatus.REGISTRATION_CLOSED)
            throw new InvalidOperationException($"Chỉ có thể bắt đầu khi cuộc thi đang là [{CompetitionStatus.REGISTRATION_CLOSED}].");

        if (!Rounds.Any(r => r.Status == RoundStatus.Pending))
            throw new InvalidOperationException("Không thể bắt đầu cuộc thi khi không có vòng đấu nào ở trạng thái [Pending].");

        Status = CompetitionStatus.ONGOING;
        UpdatedAt = now;
    }

    public void SystemFinishCompetition(DateTime now)
    {
        EnsureStatus(CompetitionStatus.ONGOING);
        Status = CompetitionStatus.FINISHED;
        UpdatedAt = now;
    }

    public void SystemInvalidCompetition(CompetitionInvalidReason reason, DateTime now)
    {
        if (Status == CompetitionStatus.FINISHED
            || Status == CompetitionStatus.RESULT_PUBLISHED)
            throw new InvalidOperationException($"Không thể [{CompetitionStatus.INVALID}] khi đã kết thúc.");

        Status = CompetitionStatus.INVALID;
        InvalidReason = reason;
        InvalidAt = now;
        UpdatedAt = now;
    }

    public void SystemPublish(DateTime now)
    {
        EnsureStatus(CompetitionStatus.DRAFT);
        Status = CompetitionStatus.PUBLISHED;
        UpdatedAt = now;
    }

    private void EnsureStatus(CompetitionStatus requiredStatus)
    {
        if (Status != requiredStatus)
            throw new InvalidOperationException($"Trạng thái cuộc thi phải là [{requiredStatus}].");
    }

    private void SetUpdated(Guid updatedBy, DateTime now)
    {
        UpdatedBy = updatedBy;
        UpdatedAt = now;
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

    private static void ValidateTimeline(
    DateTime visibleAt,
    DateTime regStart,
    DateTime regEnd,
    DateTime start,
    DateTime end)
    {
        if (visibleAt > regStart)
            throw new ArgumentException("Thời gian công bố cuộc thi phải trước thời gian đăng kí thi");

        if (regEnd > start)
            throw new ArgumentException("Thời gian kết thúc đăng kí phải trước thời gian bắt đầu cuộc thi");

        if (start >= end)
            throw new ArgumentException("Thời gian bắt đầu cuộc thi phải trước thời gian kết thúc");
    }
}