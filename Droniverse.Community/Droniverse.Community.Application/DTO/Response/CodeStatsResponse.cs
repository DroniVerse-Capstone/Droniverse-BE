namespace Droniverse.Community.Application.DTO.Response;

/// <summary>
/// Thống kê mã code activation — tổng phát hành, đã dùng, còn khả dụng, hết hạn.
/// </summary>
public class CodeStatsOverviewResponse
{
    /// <summary>Tổng số code đã phát hành.</summary>
    public int TotalCodesIssued { get; set; }

    /// <summary>Số code đã được sử dụng.</summary>
    public int UsedCodes { get; set; }

    /// <summary>Số code còn khả dụng (chưa dùng, chưa hết hạn).</summary>
    public int AvailableCodes { get; set; }

    /// <summary>Số code đã hết hạn.</summary>
    public int ExpiredCodes { get; set; }

    /// <summary>Tỉ lệ sử dụng (UsedCodes / TotalCodesIssued * 100).</summary>
    public double UsageRate { get; set; }
}
