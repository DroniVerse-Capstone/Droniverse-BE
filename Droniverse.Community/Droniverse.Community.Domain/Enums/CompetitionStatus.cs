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
        /// Cuộc thi đã được publish bởi admin.
        /// Cho phép hiển thị ra hệ thống theo timeline (VisibleAt).
        /// Bắt buộc phải có tối thiểu 1 Round đang là Pending 
        /// Người dùng có thể xem hoặc tham gia tùy thuộc vào lifecycle (thời gian).
        /// </summary>
        PUBLISHED = 1,

        /// <summary>
        /// Kết quả cuộc thi đã được công bố chính thức.
        /// Chỉ được phép khi cuộc thi đã kết thúc và hoàn tất tổng kết (IsSummarized = true).
        /// Người dùng có thể xem bảng xếp hạng và giải thưởng.
        /// Không cho phép thay đổi kết quả sau khi đã công bố.
        /// </summary>
        RESULT_PUBLISHED = 2,

        /// <summary>
        /// Cuộc thi đã bị hủy bởi admin hoặc hệ thống.
        /// Có thể xảy ra ở bất kỳ giai đoạn nào trước khi công bố kết quả.
        /// Sau khi bị hủy, người dùng không thể tiếp tục tham gia hoặc tương tác.
        /// </summary>
        CANCELLED = 3,

        /// <summary>
        /// Cuộc thi không hợp lệ do hệ thống xác định.
        /// Có thể xảy ra trong các trường hợp:
        /// - Không đủ số lượng người tham gia tối thiểu
        /// - Dữ liệu round hoặc cấu hình bị lỗi
        /// - Vi phạm rule hệ thống
        /// Trạng thái này được thiết lập tự động, không phải do người dùng.
        /// </summary>
        INVALID = 4
    }
}
