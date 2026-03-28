using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Domain.Enums
{
    public enum CompetitionStatus : byte
    {
        /// <summary>
        /// Trạng thái khởi tạo ban đầu.
        /// Cuộc thi chỉ tồn tại nội bộ, chưa hiển thị cho người dùng.
        /// Có thể chỉnh sửa toàn bộ thông tin (timeline, rule, prize, round, certificate...).
        /// </summary>
        DRAFT = 0,

        /// <summary>
        /// Cuộc thi đã được publish và bắt đầu hiển thị cho người dùng (dựa vào VisibleAt).
        /// Người dùng có thể xem thông tin cuộc thi nhưng CHƯA được đăng ký.
        /// Dùng cho giai đoạn "coming soon" hoặc chờ mở đăng ký.
        /// </summary>
        PUBLISHED = 1,

        /// <summary>
        /// Giai đoạn mở đăng ký.
        /// Người dùng có thể xem và đăng ký tham gia cuộc thi.
        /// Thường nằm trong khoảng [RegistrationStartDate, RegistrationEndDate].
        /// </summary>
        REGISTRATION_OPEN = 2,

        /// <summary>
        /// Đã đóng đăng ký.
        /// Người dùng vẫn có thể xem cuộc thi nhưng KHÔNG thể đăng ký nữa.
        /// Chuẩn bị bước sang giai đoạn thi đấu.
        /// </summary>
        REGISTRATION_CLOSED = 3,

        /// <summary>
        /// Cuộc thi đang diễn ra.
        /// Các round được thực hiện, người dùng tham gia thi đấu.
        /// Không cho phép đăng ký hoặc thay đổi thông tin quan trọng.
        /// </summary>
        ONGOING = 4,

        /// <summary>
        /// Cuộc thi đã kết thúc.
        /// Tất cả round đã hoàn thành, kết quả đã được tính toán nội bộ.
        /// Tuy nhiên CHƯA công bố kết quả cho người dùng.
        /// Thường dùng cho giai đoạn kiểm duyệt kết quả hoặc xử lý hậu kỳ.
        /// </summary>
        FINISHED = 5,

        /// <summary>
        /// Kết quả cuộc thi đã được công bố chính thức.
        /// Người dùng có thể xem ranking, giải thưởng.
        /// Không cho phép thay đổi kết quả hoặc trao thưởng thêm (trừ khi có logic đặc biệt).
        /// </summary>
        RESULT_PUBLISHED = 6,

        /// <summary>
        /// Cuộc thi đã bị hủy bởi admin hoặc hệ thống.
        /// Có thể xảy ra ở bất kỳ giai đoạn nào trước khi hoàn tất (trừ khi đã FINISHED/RESULT_PUBLISHED).
        /// Người dùng không thể tham gia hoặc tiếp tục cuộc thi.
        /// </summary>
        CANCELLED = 7,

        /// <summary>
        /// Cuộc thi không hợp lệ.
        /// Thường dùng cho các trường hợp:
        /// - Không đủ số lượng người tham gia
        /// - Dữ liệu round bị lỗi hoặc không thể tiếp tục
        /// - Vi phạm rule hệ thống
        /// Trạng thái này mang tính hệ thống (system-driven), không phải do user chủ động.
        /// </summary>
        INVALID = 8
    }
}
