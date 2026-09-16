using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Application.HttpClients;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.Services.IServices;
using Droniverse.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Droniverse.Academy.Application.Services;

/// <summary>
/// Service cho thống kê học tập toàn cục
/// Cung cấp KPIs, top clubs, top courses, và learning trends
/// </summary>
internal class LearningStatisticsService : ILearningStatisticsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly CommunityMicroserviceClient _communityMicroserviceClient;

    public LearningStatisticsService(
        IUnitOfWork unitOfWork,
        IClock clock,
        CommunityMicroserviceClient communityMicroserviceClient)
    {
        _unitOfWork = unitOfWork;
        _clock = clock;
        _communityMicroserviceClient = communityMicroserviceClient;
    }

    /// <summary>
    /// Lấy thống kê học tập toàn cục
    /// </summary>
    public async Task<LearningStatisticsResponseDto> GetLearningStatisticsAsync()
    {
        // EF Core DbContext không an toàn khi chạy nhiều query đồng thời trên cùng một scope.
        // Vì service này dùng chung UnitOfWork/DbContext, cần chạy tuần tự để tránh lỗi concurrency.
        var summary = await GetSummaryAsync();
        var topClubs = await GetTopClubsAsync();
        var courseStats = await GetCourseStatsAsync();
        var weeklyActivity = await GetWeeklyActivityAsync();

        return new LearningStatisticsResponseDto(
            Summary: summary,
            TopClubs: topClubs,
            CourseStats: courseStats,
            WeeklyActivity: weeklyActivity
        );
    }

    /// <summary>
    /// Lấy tóm tắt KPIs tổng quan
    /// </summary>
    private async Task<SummaryDto> GetSummaryAsync()
    {
        var enrollments = await _unitOfWork.Enrollments.GetAllAsync(pageSize: int.MaxValue);
        var enrollmentsList = enrollments.Data?.ToList() ?? [];

        if (enrollmentsList.Count == 0)
        {
            return new SummaryDto(
                TotalEnrollments: 0,
                AvgGlobalProgress: 0,
                TotalCertificates: 0,
                ActiveLearners30Days: 0
            );
        }

        // Tính các KPI
        int totalEnrollments = enrollmentsList.Count;
        decimal avgGlobalProgress = enrollmentsList.Count > 0
            ? Convert.ToDecimal(enrollmentsList.Average(e => e.Progress))
            : 0;

        int totalCertificates = await GetTotalCertificatesAsync();
        int activeLearners30Days = await GetActiveLearnersAsync(30);

        return new SummaryDto(
            TotalEnrollments: totalEnrollments,
            AvgGlobalProgress: Math.Round(avgGlobalProgress, 2),
            TotalCertificates: totalCertificates,
            ActiveLearners30Days: activeLearners30Days
        );
    }

    /// <summary>
    /// Tính tỷ lệ hoàn thành trung bình toàn hệ thống
    /// </summary>
    public async Task<decimal> GetAverageProgressAsync()
    {
        var enrollments = await _unitOfWork.Enrollments.GetAllAsync(pageSize: int.MaxValue);
        var enrollmentsList = enrollments.Data?.ToList() ?? [];

        if (enrollmentsList.Count == 0)
            return 0;

        return Math.Round(Convert.ToDecimal(enrollmentsList.Average(e => e.Progress)), 2);
    }

    /// <summary>
    /// Đếm số học viên đang hoạt động trong N ngày gần nhất
    /// </summary>
    public async Task<int> GetActiveLearnersAsync(int days = 30)
    {
        var cutoffDate = _clock.Now.AddDays(-days);
        
        // Lấy tất cả UserLessons có LastAccessDate >= cutoffDate
        var userLessons = await _unitOfWork.UserLessons.GetAllAsync(
            filter: ul => ul.LastAccessDate >= cutoffDate,
            pageSize: int.MaxValue
        );

        var lessons = userLessons.Data?.ToList() ?? [];
        
        // Distinct UserIDs
        var activeUserIds = lessons
            .Select(ul => ul.UserID)
            .Distinct()
            .Count();

        return activeUserIds;
    }

    /// <summary>
    /// Lấy top N câu lạc bộ có tỷ lệ hoàn thành cao nhất
    /// </summary>
    public async Task<IEnumerable<TopClubDto>> GetTopClubsAsync(int topCount = 5)
    {
        var enrollments = await _unitOfWork.Enrollments.GetAllAsync(pageSize: int.MaxValue);
        var enrollmentsList = enrollments.Data?.ToList() ?? [];

        if (enrollmentsList.Count == 0)
            return [];

        var topClubs = enrollmentsList
            .GroupBy(e => e.ClubID)
            .Select(g => new
            {
                ClubId = g.Key,
                AvgProgress = g.Average(e => e.Progress),
                MembersCount = g.Select(e => e.UserID).Distinct().Count(),
                EnrollmentsCount = g.Count()
            })
            .OrderByDescending(c => c.AvgProgress)
            .Take(topCount)
            .ToList();

        // TODO: Lấy tên câu lạc bộ từ Community Microservice
        // Hiện tại, chúng ta chỉ có ClubID, cần gọi API để lấy tên
        var clubInfos = await _communityMicroserviceClient.GetClubInfoBulkAsync(topClubs.Select(club => club.ClubId));
        var clubInfoMap = clubInfos.ToDictionary(club => club.ClubId, club => club);

        var result = topClubs.Select(club =>
        {
            clubInfoMap.TryGetValue(club.ClubId, out var clubInfo);

            var clubName = clubInfo?.ClubNameVN;
            if (string.IsNullOrWhiteSpace(clubName))
                clubName = $"Club {club.ClubId:N}";

            return new TopClubDto(
                ClubName: clubName,
                ClubImageUrl: clubInfo?.ImageUrl,
                AvgProgress: Convert.ToDecimal(Math.Round(club.AvgProgress, 2)),
                MembersCount: club.MembersCount,
                ClubId: club.ClubId
            );
        }).ToList();

        return result;
    }

    /// <summary>
    /// Lấy thống kê khóa học phổ biến (theo enrollment) và hoàn thành cao
    /// </summary>
    public async Task<IEnumerable<CourseStatsDto>> GetCourseStatsAsync(int topCount = 10)
    {
        var enrollments = await _unitOfWork.Enrollments.GetAllAsync(
            includeProperties: "CourseVersion,Course",
            pageSize: int.MaxValue
        );
        var enrollmentsList = enrollments.Data?.ToList() ?? [];

        if (enrollmentsList.Count == 0)
            return [];

        // Group by Course
        var courseStats = enrollmentsList
            .GroupBy(e => new { e.CourseID, CourseName = e.CourseVersion?.TitleVN ?? "Unknown" })
            .Select(g => new
            {
                CourseId = g.Key.CourseID,
                CourseName = g.Key.CourseName,
                Enrollments = g.Count(),
                CompletionRate = g.Where(e => e.Status == EnrollStatus.COMPLETED).Count() * 100.0 / g.Count()
            })
            .OrderByDescending(c => c.Enrollments)
            .ThenByDescending(c => c.CompletionRate)
            .Take(topCount)
            .ToList();

        return courseStats.Select(cs => new CourseStatsDto(
            CourseName: cs.CourseName,
            Enrollments: cs.Enrollments,
            CompletionRate: Convert.ToDecimal(Math.Round(cs.CompletionRate, 2)),
            CourseId: cs.CourseId
        )).ToList();
    }

    /// <summary>
    /// Lấy xu hướng học tập hàng ngày (N ngày gần nhất)
    /// </summary>
    public async Task<IEnumerable<WeeklyActivityDto>> GetWeeklyActivityAsync(int daysBack = 7)
    {
        var cutoffDate = _clock.Now.AddDays(-daysBack).Date;
        
        var userLessons = await _unitOfWork.UserLessons.GetAllAsync(
            filter: ul => ul.LastAccessDate.HasValue && ul.LastAccessDate.Value.Date >= cutoffDate,
            pageSize: int.MaxValue
        );

        var lessons = userLessons.Data?.ToList() ?? [];

        // Group by date (count only completed lessons)
        var weeklyActivity = lessons
            .Where(ul => ul.LastAccessDate.HasValue && (ul.Status == UserLessonStatus.COMPLETED))
            .GroupBy(ul => ul.LastAccessDate!.Value.Date)
            .Select(g => new WeeklyActivityDto(
                Date: g.Key.ToString("yyyy-MM-dd"),
                LessonsCompleted: g.Count()
            ))
            .ToDictionary(w => w.Date);

        // Generate dates for the range
        var result = new List<WeeklyActivityDto>();
        for (int i = -daysBack; i <= 0; i++)
        {
            var date = _clock.Now.AddDays(i).Date.ToString("yyyy-MM-dd");
            
            if (weeklyActivity.TryGetValue(date, out var activity))
            {
                result.Add(activity);
            }
            else
            {
                result.Add(new WeeklyActivityDto(Date: date, LessonsCompleted: 0));
            }
        }

        return result;
    }

    /// <summary>
    /// Tính tổng số chứng chỉ đã cấp
    /// </summary>
    private async Task<int> GetTotalCertificatesAsync()
    {
        var certificates = await _unitOfWork.UserCertificates.GetAllAsync(
            filter: uc => uc.Status == UserCertificateStatus.ACHIEVED,
            pageSize: int.MaxValue
        );

        return certificates.Data?.Count() ?? 0;
    }
}
