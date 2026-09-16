namespace Droniverse.Community.Application.DTO.Response;

/// <summary>
/// Tổng quan thống kê cuộc thi — dành cho Admin hoặc Club Manager.
/// </summary>
public class CompetitionOverviewResponse
{
    /// <summary>Tổng số cuộc thi.</summary>
    public int TotalCompetitions { get; set; }

    /// <summary>Số cuộc thi ở trạng thái nháp (DRAFT).</summary>
    public int DraftCompetitions { get; set; }

    /// <summary>Số cuộc thi đã được công bố (PUBLISHED).</summary>
    public int PublishedCompetitions { get; set; }

    /// <summary>Số cuộc thi đã công bố kết quả (RESULT_PUBLISHED).</summary>
    public int CompletedCompetitions { get; set; }

    /// <summary>Số cuộc thi đã bị hủy (CANCELLED).</summary>
    public int CancelledCompetitions { get; set; }

    /// <summary>Số cuộc thi không hợp lệ (INVALID).</summary>
    public int InvalidCompetitions { get; set; }

    /// <summary>Tổng lượt người tham gia (ACTIVE) trên tất cả cuộc thi.</summary>
    public int TotalParticipants { get; set; }

    /// <summary>Số người tham gia trung bình mỗi cuộc thi.</summary>
    public double AverageParticipantsPerCompetition { get; set; }
}

/// <summary>
/// Thông tin chi tiết 1 cuộc thi trong bảng xếp hạng top cuộc thi.
/// </summary>
public class CompetitionStatItem
{
    public Guid CompetitionId { get; set; }
    public string NameVN { get; set; } = string.Empty;
    public string NameEN { get; set; } = string.Empty;
    public Droniverse.Community.Domain.Enums.CompetitionStatus CompetitionStatus { get; set; }
    public Droniverse.Community.Domain.Enums.CompetitionLifeCycleStatus? CompetitionPhase { get; set; }
    public Guid ClubId { get; set; }
    public string ClubNameVN { get; set; } = string.Empty;
    public int ParticipantCount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

/// <summary>
/// Response chứa tổng quan + danh sách top cuộc thi theo số người tham gia.
/// </summary>
public class CompetitionStatsResponse
{
    public CompetitionOverviewResponse Overview { get; set; } = new();
    public List<CompetitionStatItem> TopByParticipants { get; set; } = [];
}
