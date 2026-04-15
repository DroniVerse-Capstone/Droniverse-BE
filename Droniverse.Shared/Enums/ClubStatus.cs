namespace Droniverse.Community.Domain.Enums;
/// <summary>
/// Trạng thái hoạt động của câu lạc bộ.
/// </summary>
public enum ClubStatus
{
    /// <summary>
    /// Câu lạc bộ chưa hoạt động hoặc đã bị tắt.
    /// Không cho phép người dùng tham gia hoặc sử dụng.
    /// </summary>
    INACTIVE = 0,

    /// <summary>
    /// Câu lạc bộ đang hoạt động bình thường.
    /// Người dùng có thể tham gia và sử dụng các chức năng.
    /// </summary>
    ACTIVE = 1,

    /// <summary>
    /// Câu lạc bộ bị tạm ngưng do vi phạm hoặc vấn đề quản lý.
    /// Tạm thời hạn chế hoặc khóa các hoạt động.
    /// </summary>
    SUSPENDED = 2,

    /// <summary>
    /// Câu lạc bộ đã bị lưu trữ.
    /// Không còn sử dụng nhưng vẫn giữ lại dữ liệu để tra cứu.
    /// </summary>
    ARCHIVED = 3
}

