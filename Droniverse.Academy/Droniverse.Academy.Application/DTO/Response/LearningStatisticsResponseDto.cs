namespace Droniverse.Academy.Application.DTO.Response;

/// <summary>
/// Tóm tắt thống kê học tập toàn cục (Global Learning KPIs)
/// </summary>
public record SummaryDto(
    /// <summary>Tổng số lượt đăng ký học trên toàn hệ thống</summary>
    int TotalEnrollments,
    
    /// <summary>Tỷ lệ hoàn thành trung bình của tất cả học viên (%)</summary>
    decimal AvgGlobalProgress,
    
    /// <summary>Tổng số chứng chỉ đã cấp</summary>
    int TotalCertificates,
    
    /// <summary>Số lượng học viên đang hoạt động trong 30 ngày qua</summary>
    int ActiveLearners30Days
);

/// <summary>
/// Thống kê câu lạc bộ - Top 5 câu lạc bộ học tập tốt nhất
/// </summary>
public record TopClubDto(
    /// <summary>Tên câu lạc bộ</summary>
    string ClubName,

    /// <summary>Ảnh đại diện của câu lạc bộ</summary>
    string? ClubImageUrl,
    
    /// <summary>Tỷ lệ hoàn thành trung bình của club (%)</summary>
    decimal AvgProgress,
    
    /// <summary>Số lượng thành viên trong club</summary>
    int MembersCount,
    
    /// <summary>ID của câu lạc bộ</summary>
    Guid ClubId
);

/// <summary>
/// Thống kê khóa học - Top khóa học phổ biến
/// </summary>
public record CourseStatsDto(
    /// <summary>Tên khóa học</summary>
    string CourseName,
    
    /// <summary>Số lượng người đăng ký</summary>
    int Enrollments,
    
    /// <summary>Tỷ lệ hoàn thành (%)</summary>
    decimal CompletionRate,
    
    /// <summary>ID của khóa học</summary>
    Guid CourseId
);

/// <summary>
/// Xu hướng học tập hàng tuần
/// </summary>
public record WeeklyActivityDto(
    /// <summary>Ngày (format: yyyy-MM-dd)</summary>
    string Date,
    
    /// <summary>Số lượng bài học được hoàn thành</summary>
    int LessonsCompleted
);

/// <summary>
/// Response cho API thống kê học tập toàn cục
/// </summary>
public record LearningStatisticsResponseDto(
    /// <summary>Tóm tắt KPIs tổng quan</summary>
    SummaryDto Summary,
    
    /// <summary>Top 5 câu lạc bộ có tỷ lệ hoàn thành cao nhất</summary>
    IEnumerable<TopClubDto> TopClubs,
    
    /// <summary>Top khóa học phổ biến và có tỷ lệ hoàn thành cao</summary>
    IEnumerable<CourseStatsDto> CourseStats,
    
    /// <summary>Xu hướng hoạt động học tập hàng ngày (7 ngày gần nhất)</summary>
    IEnumerable<WeeklyActivityDto> WeeklyActivity
);
