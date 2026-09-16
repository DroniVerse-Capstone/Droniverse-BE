namespace Droniverse.Shared.Constants;

/// <summary>
/// Constants cho các Role trong hệ thống
/// </summary>
public static class Roles
{
    /// <summary>
    /// Quản trị viên hệ thống - Quyền cao nhất
    /// </summary>
    public const string Admin = "ADMIN";

    /// <summary>
    /// Quản lý hệ thống - Quản lý tổng thể các club
    /// </summary>
    public const string SystemManager = "SYSTEM_MANAGER";

    /// <summary>
    /// Quản lý câu lạc bộ - Quản lý một club cụ thể
    /// </summary>
    public const string ClubManager = "CLUB_MANAGER";

    /// <summary>
    /// Thành viên câu lạc bộ - Người dùng thông thường
    /// </summary>
    public const string ClubMember = "CLUB_MEMBER";

    // ===== Combined Roles for Authorization =====

    /// <summary>
    /// Admin + SystemManager
    /// </summary>
    public const string AdminOrSystemManager = $"{Admin},{SystemManager}";

    /// <summary>
    /// Admin + ClubManager + SystemManager
    /// </summary>
    public const string AdminOrManagerRoles = $"{Admin},{ClubManager},{SystemManager}";

    /// <summary>
    /// ClubManager + ClubMember
    /// </summary>
    public const string ClubRoles = $"{ClubManager},{ClubMember}";

    /// <summary>
    /// Admin + SystemManager + ClubManager
    /// </summary>
    public const string SystemRoles = $"{Admin},{SystemManager},{ClubManager}";

    /// <summary>
    /// All roles
    /// </summary>
    public const string AllRoles = $"{Admin},{SystemManager},{ClubManager},{ClubMember}";
}
