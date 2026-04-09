namespace Droniverse.Community.Domain.Enums;
/// <summary>
/// Trạng thái nghiệp vụ của Round (business status),
/// thể hiện round có hợp lệ để sử dụng hay không.
/// </summary>
public enum RoundStatus : byte
{
    /// <summary>
    /// Round hợp lệ và sẵn sàng sử dụng.
    /// 
    /// Điều kiện:
    /// - StartTime < EndTime
    /// - Không bị overlap với round khác
    /// - RoundNumber đúng thứ tự (1 → n, không trùng/thiếu)
    /// - Không bị hủy
    /// 
    /// Ý nghĩa:
    /// - Có thể đưa vào competition
    /// - Có thể chạy khi tới thời gian (kết hợp lifecycle)
    /// </summary>
    Valid = 0,

    /// <summary>
    /// Round bị lỗi về lịch hoặc cấu trúc.
    /// 
    /// Khi xảy ra:
    /// - StartTime >= EndTime
    /// - Bị overlap với round khác
    /// - RoundNumber bị trùng / thiếu / sai thứ tự
    /// - Vi phạm rule hệ thống
    /// 
    /// Ý nghĩa:
    /// - Không được phép start competition
    /// - Cần user chỉnh sửa lại
    /// - Đây là lỗi do system detect (không phải do user chủ động)
    /// </summary>
    ScheduleInvalid = 1,

    /// <summary>
    /// Round bị hủy có chủ đích.
    /// 
    /// Khi xảy ra:
    /// - Admin/Organizer hủy round
    /// - Competition bị hủy → round bị hủy theo
    /// - Lý do nghiệp vụ (không đủ người, thay đổi kế hoạch...)
    /// 
    /// Ý nghĩa:
    /// - Round không còn được sử dụng
    /// - Không chạy, không tính kết quả
    /// - Không cần chỉnh sửa lại
    /// </summary>
    Cancelled = 2
}