using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Controllers;

/// <summary>
/// Controller cho các thống kê và báo cáo hệ thống học tập
/// </summary>
[Route("academy/system")]
[ApiController]
public class LearningStatisticsController : ControllerBase
{
    private readonly ILogger<LearningStatisticsController> _logger;
    private readonly ILearningStatisticsService _learningStatisticsService;

    public LearningStatisticsController(
        ILogger<LearningStatisticsController> logger,
        ILearningStatisticsService learningStatisticsService)
    {
        _logger = logger;
        _learningStatisticsService = learningStatisticsService;
    }

    /// <summary>
    /// Lấy thống kê học tập toàn cục (Global Learning Statistics).
    /// 
    /// Bao gồm:
    /// - **Summary**: Tổng enrollments, avg progress, certificates, active learners
    /// - **Top Clubs**: Top 5 câu lạc bộ có tỷ lệ hoàn thành cao nhất
    /// - **Course Stats**: Khóa học phổ biến (enrollment cao) và hoàn thành cao
    /// - **Weekly Activity**: Xu hướng hoạt động học tập hàng ngày (7 ngày gần nhất)
    /// </summary>
    /// <remarks>
    /// Endpoint này cung cấp overview toàn hệ thống về hoạt động học tập.
    /// 
    /// **Dữ liệu trả về:**
    /// - `summary.totalEnrollments`: Tổng số lượt đăng ký học
    /// - `summary.avgGlobalProgress`: Tỷ lệ hoàn thành trung bình (%)
    /// - `summary.totalCertificates`: Số lượng chứng chỉ đã cấp
    /// - `summary.activeLearners30Days`: Số người học hoạt động trong 30 ngày
    /// - `topClubs`: Danh sách top 5 club có avg progress cao nhất
    /// - `courseStats`: Top 10 khóa học (sort by enrollments desc, then completion rate desc)
    /// - `weeklyActivity`: Biểu đồ hoạt động 7 ngày gần nhất
    /// </remarks>
    /// <returns>
    /// Object chứa summary KPIs, top clubs, course stats, và weekly activity
    /// </returns>
    /// <response code="200">Lấy thống kê thành công</response>
    /// <response code="500">Lỗi server</response>
    [HttpGet("learning-statistics")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(SuccessResponse<LearningStatisticsResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLearningStatistics()
    {
        try
        {
            _logger.LogInformation("Fetching global learning statistics...");
            
            var statistics = await _learningStatisticsService.GetLearningStatisticsAsync();
            
            _logger.LogInformation("Successfully retrieved global learning statistics. " +
                "Total enrollments: {TotalEnrollments}, Active learners (30d): {ActiveLearners}, " +
                "Certificates: {TotalCertificates}",
                statistics.Summary.TotalEnrollments,
                statistics.Summary.ActiveLearners30Days,
                statistics.Summary.TotalCertificates);

            return Ok(SuccessResponse<LearningStatisticsResponseDto>.Create(
                statistics,
                "Lấy thống kê học tập toàn cục thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy thống kê học tập toàn cục.");
            throw;
        }
    }

    /// <summary>
    /// Lấy tỷ lệ hoàn thành trung bình toàn hệ thống.
    /// </summary>
    /// <returns>Phần trăm hoàn thành trung bình</returns>
    /// <response code="200">Lấy thành công</response>
    [HttpGet("average-progress")]
    [ProducesResponseType(typeof(SuccessResponse<decimal>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAverageProgress()
    {
        try
        {
            var avgProgress = await _learningStatisticsService.GetAverageProgressAsync();
            return Ok(SuccessResponse<decimal>.Create(
                avgProgress,
                "Lấy tỷ lệ hoàn thành trung bình thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy tỷ lệ hoàn thành trung bình.");
            throw;
        }
    }

    /// <summary>
    /// Lấy số lượng học viên đang hoạt động.
    /// </summary>
    /// <param name="days">Số ngày tính từ hôm nay (mặc định: 30)</param>
    /// <returns>Số lượng học viên</returns>
    /// <response code="200">Lấy thành công</response>
    [HttpGet("active-learners")]
    [ProducesResponseType(typeof(SuccessResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveLearners([FromQuery] int days = 30)
    {
        try
        {
            var count = await _learningStatisticsService.GetActiveLearnersAsync(days);
            return Ok(SuccessResponse<int>.Create(
                count,
                $"Lấy số học viên hoạt động trong {days} ngày thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy số học viên hoạt động.");
            throw;
        }
    }

    /// <summary>
    /// Lấy top N câu lạc bộ có tỷ lệ hoàn thành cao nhất.
    /// </summary>
    /// <param name="topCount">Số lượng câu lạc bộ cần lấy (mặc định: 5)</param>
    /// <returns>Danh sách top clubs</returns>
    /// <response code="200">Lấy thành công</response>
    [HttpGet("top-clubs")]
    [ProducesResponseType(typeof(SuccessResponse<IEnumerable<TopClubDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopClubs([FromQuery] int topCount = 5)
    {
        try
        {
            var topClubs = await _learningStatisticsService.GetTopClubsAsync(topCount);
            return Ok(SuccessResponse<IEnumerable<TopClubDto>>.Create(
                topClubs,
                "Lấy top câu lạc bộ thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy top câu lạc bộ.");
            throw;
        }
    }

    /// <summary>
    /// Lấy thống kê khóa học phổ biến và hoàn thành cao.
    /// </summary>
    /// <param name="topCount">Số lượng khóa học cần lấy (mặc định: 10)</param>
    /// <returns>Danh sách thống kê khóa học</returns>
    /// <response code="200">Lấy thành công</response>
    [HttpGet("course-stats")]
    [ProducesResponseType(typeof(SuccessResponse<IEnumerable<CourseStatsDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCourseStats([FromQuery] int topCount = 10)
    {
        try
        {
            var courseStats = await _learningStatisticsService.GetCourseStatsAsync(topCount);
            return Ok(SuccessResponse<IEnumerable<CourseStatsDto>>.Create(
                courseStats,
                "Lấy thống kê khóa học thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy thống kê khóa học.");
            throw;
        }
    }

    /// <summary>
    /// Lấy xu hướng học tập hàng ngày.
    /// </summary>
    /// <param name="daysBack">Số ngày tính từ hôm nay (mặc định: 7)</param>
    /// <returns>Danh sách hoạt động hàng ngày</returns>
    /// <response code="200">Lấy thành công</response>
    [HttpGet("weekly-activity")]
    [ProducesResponseType(typeof(SuccessResponse<IEnumerable<WeeklyActivityDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWeeklyActivity([FromQuery] int daysBack = 7)
    {
        try
        {
            var activity = await _learningStatisticsService.GetWeeklyActivityAsync(daysBack);
            return Ok(SuccessResponse<IEnumerable<WeeklyActivityDto>>.Create(
                activity,
                "Lấy xu hướng học tập hàng ngày thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy xu hướng học tập hàng ngày.");
            throw;
        }
    }
}
