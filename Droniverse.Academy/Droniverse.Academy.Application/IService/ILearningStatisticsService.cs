using Droniverse.Academy.Application.DTO.Response;

namespace Droniverse.Academy.Application.IService;

/// <summary>
/// Service cho thống kê học tập toàn cục (Global Learning Analytics)
/// Cung cấp KPIs, top clubs, top courses, và learning trends
/// </summary>
public interface ILearningStatisticsService
{
    /// <summary>
    /// Lấy thống kê học tập toàn cục
    /// Bao gồm: KPIs, top clubs, top courses, weekly activity
    /// </summary>
    /// <returns>Dữ liệu thống kê đầy đủ</returns>
    Task<LearningStatisticsResponseDto> GetLearningStatisticsAsync();

    /// <summary>
    /// Tính tỷ lệ hoàn thành trung bình toàn hệ thống
    /// </summary>
    Task<decimal> GetAverageProgressAsync();

    /// <summary>
    /// Đếm số học viên đang hoạt động trong N ngày gần nhất
    /// </summary>
    /// <param name="days">Số ngày tính từ hôm nay</param>
    Task<int> GetActiveLearnersAsync(int days = 30);

    /// <summary>
    /// Lấy top N câu lạc bộ có tỷ lệ hoàn thành cao nhất
    /// </summary>
    /// <param name="topCount">Số lượng câu lạc bộ cần lấy (mặc định: 5)</param>
    Task<IEnumerable<TopClubDto>> GetTopClubsAsync(int topCount = 5);

    /// <summary>
    /// Lấy thống kê khóa học phổ biến (theo enrollment) và hoàn thành cao
    /// </summary>
    /// <param name="topCount">Số lượng khóa học cần lấy (mặc định: 10)</param>
    Task<IEnumerable<CourseStatsDto>> GetCourseStatsAsync(int topCount = 10);

    /// <summary>
    /// Lấy xu hướng học tập hàng ngày (N ngày gần nhất)
    /// </summary>
    /// <param name="daysBack">Số ngày tính từ hôm nay (mặc định: 7)</param>
    Task<IEnumerable<WeeklyActivityDto>> GetWeeklyActivityAsync(int daysBack = 7);
}
